using BBWM.Core.Web;

namespace BBWM.Demo;

public static class Routes
{
    private static readonly RouteBuilder DemoBuilder = new RouteBuilder("/app/demo");

    public static class GridMasterDetail
    {
        private static readonly RouteBuilder GridMasterDetailBuilder = new("/grid-master-detail", DemoBuilder);

        public static readonly Route Page = GridMasterDetailBuilder.Build("page", "Page Edit");
        public static readonly Route Create = GridMasterDetailBuilder.Build("create", "Add Order");
        public static readonly Route Edit = GridMasterDetailBuilder.Build("edit/:id", "Edit Order");
        public static readonly Route Details = GridMasterDetailBuilder.Build("details/:id", "Order Details");
        public static readonly Route Inline = GridMasterDetailBuilder.Build("inline", "Inline Edit");
        public static readonly Route Popup = GridMasterDetailBuilder.Build("popup", "Popup Edit");
    }

    public static class Guidelines
    {
        private static readonly RouteBuilder GuidelinesBuilder = new("/guidelines", DemoBuilder);

        public static readonly Route General = GuidelinesBuilder.Build("general", "General UI Rules for Developers");
        public static readonly Route Basic = GuidelinesBuilder.Build("basic", "Basic Page Structure");
        public static readonly Route Headings = GuidelinesBuilder.Build("headings", "Headings");
        public static readonly Route Lists = GuidelinesBuilder.Build("lists", "Lists");
        public static readonly Route Inputs = GuidelinesBuilder.Build("inputs", "Labels and Inputs");
        public static readonly Route Buttons = GuidelinesBuilder.Build("buttons", "Buttons");
        public static readonly Route Calendar = GuidelinesBuilder.Build("calendar", "Calendar");
        public static readonly Route Disabled = GuidelinesBuilder.Build("disabled", "Disabled Controls");
        public static readonly Route Links = GuidelinesBuilder.Build("links", "Links");
        public static readonly Route Search = GuidelinesBuilder.Build("search", "Search");
        public static readonly Route Tabs = GuidelinesBuilder.Build("tabs", "Tabs");
        public static readonly Route Panels = GuidelinesBuilder.Build("panels", "Panels");
        public static readonly Route Dialogs = GuidelinesBuilder.Build("dialogs", "Dialogs");
        public static readonly Route Grids = GuidelinesBuilder.Build("grids", "Grids");
        public static readonly Route Tree = GuidelinesBuilder.Build("tree", "Tree");
        public static readonly Route Pdf = GuidelinesBuilder.Build("pdf", "Pdf Generation");
    }

    public static readonly Route GridFilter = DemoBuilder.Build("grid-filter", "Grid Filters");
    public static readonly Route GridLocal = DemoBuilder.Build("grid-local", "Grid With Local Data");
    public static readonly Route IdHashing = DemoBuilder.Build("id-hashing", "ID Hashing");
    public static readonly Route IdHashingDetails = DemoBuilder.Build("id-hashing/details/:id", "Order details");
    public static readonly Route Impersonation = DemoBuilder.Build("impersonation", "Impersonation");
    public static readonly Route ImageUploader = DemoBuilder.Build("image-uploader", "Image Uploader");
    public static readonly Route Raygun = DemoBuilder.Build("raygun", "Raygun");
    public static readonly Route Culture = DemoBuilder.Build("culture", "Culture");
    public static readonly Route DisabledControls = DemoBuilder.Build("disabled", "Disabled Controls");
}
