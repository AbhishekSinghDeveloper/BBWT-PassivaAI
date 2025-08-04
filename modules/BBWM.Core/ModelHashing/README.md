## Common

Hashing occurs between client and server: for json - on json serialisation step, for query params - in action filters.
There is a restriction: if model type contains keys with for example names Id and EmailTemplateId, then
1. filter query from client side for this model must looks like filters[i].propertyName="id" or filters[i].propertyName="emailTemplateId"
2. route data values also must have same names as properties, "id" and "emailTemplateId"

For each property hasher takes entity this property referenced on(for primary keys - parent model) and use entity type full name as salt.
It works automatically for all CRUDControllerBase methods and for custom controllers/actions when their parameters marked by special attribute, and hashes/parse all properties mapped by automapper to database pk/fk fields, and also properties manually marked by special attribute.

## IModelHashingService

### Register
Must be called in Startup.Configure or in the implementations of IConfigureModuleLinkage if required. Will add all properties mapped by automapper to database pk/fk fields.

### IgnoreModelHashing
Can be called in Startup.ConfigureServices or in the implementations of IConfigureModuleLinkage if required. Will ignore specified model. Example - AuditDTO

### IgnorePropertiesHashing
Can be called in Startup.ConfigureServices or in the implementations of IConfigureModuleLinkage if required. Will ignore specified properties, which was added on Register step.

### ManualPropertyHashing
Can be called in Startup.ConfigureServices or in the implementations of IConfigureModuleLinkage if required. Will add custom properties, even if there is no mapping to database key. You must pass reference entity type in attribute. Example - EmailDTO.EmailTemplateId

## UnHashProperty
Implemented as a protected method of CrudServiceBase. Can be used in CrudServiceBase children if required.

## HashProperty
Implemented as a protected method of CrudControllerBase. Can be used in CrudControllerBase children if required.

## GlobalHashKeyJsonConverter
Used to work with json data - on read/write json from/to client side, check if model type added/not ignored on previous step, then parse/hash keys.
It runs automatically when the model parameter is marked with [FromBody] attribute.

## IdBinderAttribute
Used to parse keys properties from query string in CRUDControllerBase children, takes model type from generic argument, then check if model type added/not ignored on previous steps, then parse/hash keys. The parameter name for key must be the same as property name from model. Example - CRUDControllerBase.

## IdBinder
Used to parse keys properties before the model is passed into custom controllers/actions when the related parameter is marked with [FromForm] or [FromQuery] attributes.
You must set it in the ModelBinder attribute for the property of the model. Example - EmailDTO.EmailTemplateId or ObjectReferenceFilter.Value.