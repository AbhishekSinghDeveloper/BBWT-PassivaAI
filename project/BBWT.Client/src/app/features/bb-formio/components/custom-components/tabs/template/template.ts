import { TabState } from "../interfaces";

interface Tab {
  key: string;
  label: string;
  state: number;
}

interface Context {
  component: {
    verticalLayout: boolean;
    components: Tab[];
  };
  currentTab: number;
  tabLikey: string;
  tabLinkKey: string;
  tabKey: string;
  tabComponents: string[];
  t: (label: string, options: { _userInput: boolean }) => string;
}

export default function tabTemplate(ctx: Context): string {
  let __p = "";
  const __j = Array.prototype.join;

  __p += `<div class="card${ctx.component.verticalLayout ? " card-vertical" : ""}">
      <div class="card-header">
        <ul class="tab-component-tabs nav nav-tabs card-header-tabs${ctx.component.verticalLayout ? " nav-tabs-vertical" : ""}" role="tablist">`;

  ctx.component.components.forEach((tab, index) => {
    __p += `
          <li class="nav-item${ctx.currentTab === index ? " active" : ""}"
            role="tab"
            ref="${ctx.tabLikey}"
            style="display: ${tab.state == TabState.Hide ? "none" : "block"} ">
            <a class="nav-link${ctx.currentTab === index ? " active" : ""}${ctx.component.verticalLayout ? " nav-link-vertical" : ""}"
                href="#${tab.key}" ref="${ctx.tabLinkKey}">
              ${ctx.t(tab.label, { _userInput: true })}
            </a>
          </li>`;
  });

  __p += `
        </ul>
      </div>`;

  ctx.component.components.forEach((tab, index) => {
    __p += `
      <div role="tabpanel" class="card-body tab-pane" style="display: ${(ctx.currentTab === index && tab.state != TabState.Hide) ? "block" : "none"}" ref="${ctx.tabKey}">
        ${ctx.tabComponents[index]}
      </div>`;
  });

  __p += `
    </div>`;

  return __p;
}