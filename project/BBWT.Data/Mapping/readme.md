It's recommended that you place a model<->DTO mapping directly into the model's class. It's an easier way comparing
to a separate mapping profile's class. Simply add:

        public static void RegisterMap(IMapperConfigurationExpression c)
        {
            c.CreateMap<TheModel, TheModelDTO>().ReverseMap();
        }

into your model class. This method is automatically found and handled by the BBWT's core code.

***
Otherwise, in any complex case of mapping you can place a separate mapping profile into this folder.
