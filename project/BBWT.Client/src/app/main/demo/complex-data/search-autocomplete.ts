import { StringFilter, StringFilterMatchMode } from "@features/filter";
import { PagedCrudService } from "@features/grid";

export async function searchAutocompleteResults<TResult>(
    searchTerm: string,
    service: PagedCrudService<TResult>,
    field: string,
    sortField?: string
) {
    const sortingField = sortField || field;
    const filter = new StringFilter(field, searchTerm, StringFilterMatchMode.Contains);

    const page = await service.getPage({
        filters: [filter],
        sortingField,
        take: 30
    });

    return page.items;
}
