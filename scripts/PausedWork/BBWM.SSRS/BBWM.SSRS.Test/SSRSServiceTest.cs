using AutoMapper;
using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Test;
using BBWM.Core.Test.Crud;
using BBWM.JWT;
using BBWT.Data;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace BBWM.SSRS.Test
{
    public class SSRSServiceTest : PagedCrudServiceTestBase<ISsrsService, CatalogDTO, Guid>
    {
        private readonly IMapper mapper;
        private readonly ISsrsDataContext context;

        public SSRSServiceTest()
        {
            this.mapper = AutoMapperConfig.CreateMapper();
            this.context = GenerateSsrsDataContext();
        }


        protected override void ChangeAttribute(CatalogDTO entity)
        {
            entity.Name += " new";
        }

        protected override CatalogDTO GetEntity()
        {
            var faker = new Faker<CatalogDTO>()
                .RuleFor(p => p.Id, s => Guid.NewGuid())
                .RuleFor(p => p.Name, s => s.Random.Word())
                .RuleFor(p => p.Path, s => s.Random.Words());

            return faker.Generate();
        }

        protected override IEnumerable<FilterInfoBase> GetFilters()
        {
            throw new NotImplementedException();
        }

        protected override ISsrsService GetService<TContext>(TContext context)
        {
            var jwtMock = new Mock<IJwtService>();
            var userServiceMock = new Mock<ICurrentUserService>();

            return new SsrsService(this.context, mapper, jwtMock.Object, userServiceMock.Object);
        }

        private ISsrsDataContext GenerateSsrsDataContext(string dbName = null)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = Guid.NewGuid().ToString();
            }

            var options = new DbContextOptionsBuilder<SsrsDataContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new SsrsDataContext(options);

        }
    }
}
