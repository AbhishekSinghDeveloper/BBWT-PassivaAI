import {Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild} from "@angular/core";
import {SqlQueryBuilderService} from "../api/sql-query-builder.service";
import {MessageService, SelectItem} from "primeng/api";
import {Mode} from "highlight.js";
import {
    CodeEditorBlock,
    CodeEditorContext,
    CodeEditorDelta,
    CodeEditorKeyboard,
    CodeEditorKeyboardHandler,
    CodeEditorRange,
    CodeEditorRect,
    CodeEditorWrapper
} from "@main/reporting.v3/code-editor/code-editor.models";
import {VariablesService} from "@main/reporting.v3/api/variables.service";
import {IQuerySource, IVariable, ITableSetColumn, ITableSetFolderInfo, ITableSetTable} from "@main/reporting.v3/core/reporting-models";
import {ListBoxSettings, ISqlQueryBuild} from "../query-builder-models";
import {ListboxChangeEvent} from "primeng/listbox";
import {IHash} from "@bbwt/interfaces";
import {TableSetService} from "@main/reporting.v3/api/table-set.service";
import {QueryTableSelectorComponent} from "@main/reporting.v3/components/query-table-selector.component";
import {ContextVariablesService} from "@main/reporting.v3/query-builder/api/context-variables.service";
import {Message} from "@bbwt/classes";


@Component({
    selector: "sql-editor",
    templateUrl: "./sql-editor.component.html",
    styleUrls: ["./sql-editor.component.scss"]
})
export class SqlEditorComponent implements OnInit {
    // Core editor settings.
    tokens: Mode[];
    editor: CodeEditorWrapper;
    keyboard: CodeEditorKeyboard;
    editorDisabled: boolean;
    sqlCode: string;

    // Column settings.
    columns: IHash<ITableSetColumn[]>;
    columnsListBox: ListBoxSettings = new ListBoxSettings(".");

    // Context variables features settings.
    contextVariables: string[];
    contextVariablesListBox: ListBoxSettings = new ListBoxSettings("@");

    // Table set features settings.
    folderId: string;

    // Overly lists settings.
    overlyListBoxes: ListBoxSettings[];

    // Tables settings.
    tables: ITableSetTable[];
    tablesListBox: ListBoxSettings = new ListBoxSettings(null);

    // Variables features settings.
    variables: IVariable[];
    variablesListBox: ListBoxSettings = new ListBoxSettings("#");

    private _sqlQueryBuild: ISqlQueryBuild = {} as ISqlQueryBuild;
    private _querySource: IQuerySource = {queryType: "sql"} as IQuerySource;

    @Output() querySourceChange: EventEmitter<IQuerySource> = new EventEmitter<IQuerySource>();

    @ViewChild(QueryTableSelectorComponent) private _queryTableSelector: QueryTableSelectorComponent;

    constructor(private contextVariablesService: ContextVariablesService,
                private querySqlService: SqlQueryBuilderService,
                private variablesService: VariablesService,
                private tableSetService: TableSetService,
                private messageService: MessageService) {
        this.overlyListBoxes = [
            this.variablesListBox,
            this.contextVariablesListBox,
            this.tablesListBox,
            this.columnsListBox
        ];
    }

    @ViewChild("tables") set tablesContainer(div: ElementRef<HTMLDivElement>) {
        this.tablesListBox.elem = div;
    }

    @ViewChild("variables") set variablesContainer(div: ElementRef<HTMLDivElement>) {
        this.variablesListBox.elem = div;
    }

    @ViewChild("columns") set columnsContainer(div: ElementRef<HTMLDivElement>) {
        this.columnsListBox.elem = div;
    }

    @ViewChild("contextVariables") set contextVariablesContainer(div: ElementRef<HTMLDivElement>) {
        this.contextVariablesListBox.elem = div;
    }

    @Input() set disabled(value: boolean) {
        this.editorDisabled = value;
    }

    get disabled(): boolean {
        // Query editor is disabled if it was disabled externally
        // or if this query is not a SQL query (has no associated SQL query edition object).
        return this.editorDisabled || !this._sqlQueryBuild;
    }

    @Input() set querySourceId(value: string) {
        if (value === this.querySourceId) return;
        if (!!value) {
            this.querySqlService.getBuild(value)
                .then(build => this.sqlQueryBuild = build)
                .catch(error => this.messageService.add(Message.Error(error.message, "Error loading sql query")));
        } else this.sqlQueryBuild = null;
    }

    get querySourceId(): string {
        return this._querySource?.id;
    }

    set sqlQueryBuild(value: ISqlQueryBuild) {
        this._sqlQueryBuild = value ?? {sqlCode: "", querySource: {queryType: "sql"}} as ISqlQueryBuild;
        this._querySource = this._sqlQueryBuild.querySource;
        this.sqlCode = this._sqlQueryBuild.sqlCode;
        this.querySourceChange.emit(this._querySource);
    }

    get sqlQueryBuild(): ISqlQueryBuild {
        return this._sqlQueryBuild;
    }

    get isDraftQuery(): boolean {
        return !!this._querySource.isDraft;
    }

    get dirty(): boolean {
        const sqlCode: string = this.sqlCode?.trim();
        const originalCode: string = this._sqlQueryBuild?.sqlCode?.trim();
        if (!sqlCode?.length && !originalCode?.length) return false;
        return !this.disabled && sqlCode !== originalCode;
    }


    async ngOnInit(): Promise<void> {
        this.columnsListBox.load = this.loadColumnListBox.bind(this, this.columnsListBox);
        this.columnsListBox.prepare = this.prepareColumnListBox.bind(this, this.columnsListBox);
        this.columnsListBox.clean = this.cleanColumnListBox.bind(this, this.columnsListBox);

        await this.refreshVariables();
        await this.refreshContextVariables();
        this.configureHighlighting();
        this.configureKeyboard();
    }

    // Configure variables highlighting.
    private configureHighlighting(): void {
        if (!this.variables) return;

        const names: string[] = this.variables.map(variable => variable.name);
        const variablesPattern: RegExp = new RegExp("\\B#(" + names.join("|") + ")\\b");

        this.tokens = [
            {
                scope: "variable",
                match: variablesPattern,
                relevance: 2
            },
            {
                scope: "unknownVariable",
                match: /\B#[a-zA-Z0-9_]+\b/,
                relevance: 1
            }];
    }

    // Configure keyboard hacks.
    private configureKeyboard(): void {
        this.keyboard = {
            bindings: {
                "indent code-block": {
                    key: 9,
                    handler: (range: CodeEditorRange, context: CodeEditorContext,
                              originalHandler?: CodeEditorKeyboardHandler): boolean => {

                        // If some overly list is visible, capture this key.
                        if (this.overlyListBoxes.some(list => list.visible)) {

                            // If this list is visible, insert its selection.
                            // If no selection has been made before, auto-select its first option.
                            this.overlyListBoxes.forEach(list => {
                                if (!list.visible) return;
                                list.selectionIndex ??= 0;
                                this.insertOverlyListSelection(list, "tab");
                            });

                            // Otherwise, call default handler for this key.
                        } else if (!!originalHandler) return originalHandler(range, context);
                    }
                },
                "enter": {
                    key: 13,
                    handler: (_: CodeEditorRange, __: CodeEditorContext, ___?: CodeEditorKeyboardHandler): boolean => {

                        // If some overly list is visible and has selection, capture this key.
                        if (this.overlyListBoxes.some(list => list.visible && list.selection != null)) {

                            // If this list met the conditions, insert its selection.
                            this.overlyListBoxes.forEach(list => {
                                if (!list.visible || list.selection == null) return;
                                this.insertOverlyListSelection(list, "enter");
                            });

                            // Otherwise, perform default behavior for this key.
                        } else return true;
                    }
                },
                "left": {
                    key: 37,
                    handler: (_: CodeEditorRange, __: CodeEditorContext, ___?: CodeEditorKeyboardHandler): boolean => {
                        // If some overly list is visible, capture this key.
                        // Otherwise, perform default behavior for this key.
                        return !this.overlyListBoxes.some(list => list.visible);
                    }
                },
                "up": {
                    key: 38,
                    handler: (_: CodeEditorRange, __: CodeEditorContext, ___?: CodeEditorKeyboardHandler): boolean => {

                        // If some overly list is visible, capture this key.
                        if (this.overlyListBoxes.some(list => list.visible)) {

                            // If this list is visible, move its selection up.
                            this.overlyListBoxes.forEach(list => {
                                if (!list.visible) return;
                                list.selectionIndex--;
                            });

                            // Otherwise, perform default behavior for this key.
                        } else return true;
                    }
                },
                "right": {
                    key: 39,
                    handler: (_: CodeEditorRange, __: CodeEditorContext, ___?: CodeEditorKeyboardHandler): boolean => {
                        // If some overly list is visible, capture this key.
                        // Otherwise, perform default behavior for this key.
                        return !this.overlyListBoxes.some(list => list.visible);
                    }
                },
                "down": {
                    key: 40,
                    handler: (_: CodeEditorRange, __: CodeEditorContext, ___?: CodeEditorKeyboardHandler): boolean => {

                        // If some overly list is visible, capture this key.
                        if (this.overlyListBoxes.some(list => list.visible)) {

                            // If this list is visible, move its selection up.
                            this.overlyListBoxes.forEach(list => {
                                if (!list.visible) return;
                                list.selectionIndex++;
                            });

                            // Otherwise, perform default behavior for this key.
                        } else return true;
                    }
                },
                "show tables list": {
                    key: 32,
                    ctrlKey: true,
                    handler: (range: CodeEditorRange, __: CodeEditorContext, ___?: CodeEditorKeyboardHandler): boolean => {
                        if (!range) return true;
                        const index: number = range.index;
                        const position: CodeEditorRect = this.editor.getBounds(index);
                        this.hideOverlyLists();
                        this.tablesListBox.show(position, index);
                    }
                }
            }
        }
    }

    // Refreshing methods.
    private async refreshContextVariables(): Promise<void> {
        this.contextVariables = (await this.contextVariablesService.getVariableNames()) ?? [];
        this.contextVariablesListBox.options = this.contextVariables.map(variableName => <SelectItem>{
            label: this.getDisplayableLabel(variableName),
            value: variableName
        });
    }

    private async refreshVariables(): Promise<void> {
        this.variables = await this.variablesService.getAll() ?? [];

        // Get unique variable names.
        const names: string[] = Array.from(new Set(this.variables.map(variables => variables.name))).sort();

        this.variablesListBox.options = names.map(name => <SelectItem>{
            label: this.getDisplayableLabel(name),
            value: name
        });
    }

    protected refreshTableSetTables(folderInfo: ITableSetFolderInfo): void {
        this.folderId = folderInfo?.id;
        this.tables = folderInfo?.tables ?? [];
        this.tablesListBox.options = this.tables.map(table => <SelectItem>{
            label: this.getDisplayableLabel(table.tableAlias),
            value: table.tableAlias
        });
        this.columns = {};
    }

    private async refreshColumns(table: ITableSetTable): Promise<void> {
        if (!table) return;
        await this.tableSetService.getTableColumns(table.sourceCode, this.folderId, table.id)
            .then(columns => this.columns[table.id] = columns);
    }

    // Auxiliary methods.
    private getDisplayableLabel(label: string): string {
        if (!label) return "";
        return label.length <= 50 ? label : `${label.slice(0, 50)}...`;
    }

    // Code editor external features methods.
    protected configureExternalHandlers(editor: CodeEditorWrapper): void {
        if (!editor) return;

        this.editor = editor;
        this.overlyListBoxes.forEach(list => this.editor.appendChild(list.elem.nativeElement));

        // Check for # every time text changes.
        editor.on("text-change", this.checkForSpecialCharacter.bind(this));

        // Hide the variables list if editor scrolls.
        editor.on("scroll", this.hideOverlyLists.bind(this));

        // Hide the variables list if editor lost focus, except when focus on the list of variables.
        editor.on("focusout", this.focusLost.bind(this));
    }

    private focusLost(event: FocusEvent): void {
        const relatedTarget: Element = event?.relatedTarget as Element;
        const classList: string = relatedTarget?.classList?.value;
        if (!classList?.includes("p-listbox")) this.hideOverlyLists();
    }

    private checkForSpecialCharacter(delta: CodeEditorDelta, _: any, source: string): void {
        if (source !== "user" || !delta?.ops?.length) return;

        // Inserted key if any key was pressed, otherwise null.
        const insert: string = delta.ops[delta.ops.length - 1].insert;

        // If some overly list is shown or some special key was typed.
        if (this.overlyListBoxes.some(list => list.index != null || insert === list.term)) {

            // Get editor text selection range.
            const selection: CodeEditorRange = this.editor.getSelection();
            if (!selection) return;
            const index: number = selection.index;

            // Get the position of the end of the selection.
            const position: CodeEditorRect = this.editor.getBounds(index);
            if (!position) return;

            this.overlyListBoxes.forEach(list => {
                // If this overly list is shown.
                if (list.index != null) {
                    // If the inserted character is not a valid character, hide the list.
                    if (!!insert && !insert.match(/[a-zA-Z0-9_-]+/)) list.hide();
                    // If the special character is removed, hide the list.
                    else if (index < list.index) list.hide();
                    // Otherwise, refresh filtering term and move the list.
                    else {
                        const [line, offset]: [CodeEditorBlock, number] = this.editor.getLine(index);
                        const text: string = line.domNode.textContent;
                        list.update(position, index, text, offset);
                    }
                }

                // If the character inserted is the special character of this list, show the list.
                if (insert === list.term) {
                    this.hideOverlyLists();
                    list.show(position, index);
                }
            });
        }
    }

    protected insertOverlyListSelection(list: ListBoxSettings, source: string, event?: ListboxChangeEvent): void {
        if (!list?.visible || !event?.value && !list?.selection) return;

        list.prepare(source).then(_ => {
            const value: string = event?.value ?? list.selection;
            const length: number = list.filter.length + list.deletion;
            const index: number = list.index - list.deletion;

            if (length) this.editor.deleteText(index, length);
            this.editor.insertText(index, value);

            list.hide();
        });
    }

    protected insertTableNodeValue(table: ITableSetTable): void {
        if (!table?.tableAlias) return;

        const selection: CodeEditorRange = this.editor.getSelection();

        if (this.sqlCode.trim().length === 0) {
            this.editor.insertText(0, `SELECT * FROM ${table.tableAlias}`);

        } else this.editor.insertText(selection?.index ?? 0, table.tableAlias);

        this.hideOverlyLists();
    }

    protected insertColumnNodeValue(column: ITableSetColumn): void {
        if (!column?.columnAlias || !column?.table?.tableAlias) return;

        const selection: CodeEditorRange = this.editor.getSelection();

        if (this.sqlCode.trim().length === 0) {
            this.editor.insertText(0, `SELECT ${column.table.tableAlias}.${column.columnAlias} FROM ${column.table.tableAlias}`);

        } else this.editor.insertText(selection?.index ?? 0, `${column.table.tableAlias}.${column.columnAlias}`);

        this.hideOverlyLists();
    }

    private hideOverlyLists(): void {
        if (!this.overlyListBoxes?.length) return;
        this.overlyListBoxes.forEach(list => list.hide());
    }

    private async loadColumnListBox(list: ListBoxSettings): Promise<void> {
        if (!list?.index || !this.folderId) return;

        const [line, offset]: [CodeEditorBlock, number] = this.editor.getLine(list.index);
        const text: string = line.domNode.textContent;
        const index: number = text.substring(0, offset).search(/[a-zA-Z0-9_-]+\.$/);
        const name: string = text.substring(index, offset - 1);
        const pattern: RegExp = new RegExp(name, "i");
        const table: ITableSetTable = this.tables.find(table =>
            table.tableAlias.length === name.length && table.tableAlias.search(pattern) === 0);

        if (!table) return;
        if (!this.columns[table.id]) await this.refreshColumns(table);

        const columns: ITableSetColumn[] = this.columns[table.id];
        if (!columns) return;

        this.columnsListBox.deletion = table.tableAlias.length + 1;
        this.columnsListBox.options = columns.map(column => <SelectItem>{
            label: this.getDisplayableLabel(column.columnAlias),
            value: column.columnAlias
        });
    }

    private async prepareColumnListBox(list: ListBoxSettings, source: string): Promise<void> {
        if (source.toLowerCase() !== "tab") list.deletion = 0;
    }

    private async cleanColumnListBox(): Promise<void> {
        this.columnsListBox.deletion = 0;
        this.columnsListBox.options = null;
    }

    // Edition methods.
    async getSqlQueryBuild(): Promise<ISqlQueryBuild> {
        if (!this.sqlQueryBuild) return null;

        const tableSetId: string = await this._queryTableSelector.save();

        return {
            id: this.sqlQueryBuild?.id,
            sqlCode: this.sqlCode,
            tableSetId: tableSetId,
            querySource: this._querySource,
            querySourceId: this._querySource?.id
        };
    }

    async createDraft(): Promise<string> {
        const querySourceId: string = this.isDraftQuery ? this._querySource.releaseQueryId : this.querySourceId;
        const editionFunc = (sqlQuery: ISqlQueryBuild): Promise<ISqlQueryBuild> =>
            this.querySqlService.createDraft(sqlQuery, querySourceId);
        return this.editQuery(editionFunc);
    }

    async releaseDraft(): Promise<string> {
        if (!this.querySourceId) return;

        const editionFunc = (sqlQuery: ISqlQueryBuild): Promise<ISqlQueryBuild> =>
            this.querySqlService.update(sqlQuery.id, sqlQuery)
                .then(build => this.querySqlService.releaseDraft(build.querySourceId)
                    .then(querySourceId => this.querySqlService.getBuild(querySourceId)));

        return this.editQuery(editionFunc);
    }

    async save(): Promise<string> {
        const editionFunc = (sqlQuery: ISqlQueryBuild): Promise<ISqlQueryBuild> =>
            !this.querySourceId
                ? this.querySqlService.create(sqlQuery)
                : this.querySqlService.update(sqlQuery.id, sqlQuery);

        return this.editQuery(editionFunc);
    }

    private async editQuery(editionFunc: (sqlQuery: ISqlQueryBuild) => Promise<ISqlQueryBuild>): Promise<string> {
        const sqlQuery: ISqlQueryBuild = await this.getSqlQueryBuild();

        // Try to edit the query. Restore sql query if edition fails.
        const build: ISqlQueryBuild = await editionFunc(sqlQuery).catch(error => {
            this.messageService.add(Message.Error(`There was an error saving entity:\n${error.error}`));
            return null;
        });

        // Update the build only if there was no error.
        if (build != null) this.sqlQueryBuild = build;

        return build?.querySourceId ?? null;
    }
}


