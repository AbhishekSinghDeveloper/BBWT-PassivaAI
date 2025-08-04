using Microsoft.EntityFrameworkCore;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;

namespace BBWM.FormIO.Services
{
    public class FormIOCategoryService : IFormIOCategoryService
    {
        private readonly IDbContext _context;
        private readonly IDataService _dataService;

        public FormIOCategoryService(
            IDbContext context,
            IDataService dataService)
        {
            _context = context;
            _dataService = dataService;
        }

        public async Task<FormCategoryDTO> Create(FormCategoryDTO dto, CancellationToken ct = default)
        {
            if (await _dataService.Context.Set<FormCategory>().AnyAsync(category => category.Name == dto.Name, ct))
            {
                throw new BusinessException("A category with the same name already exists");
            }

            return await _dataService.Create<FormCategory, FormCategoryDTO>(dto, ct);
        }

        public async Task Delete(int id, CancellationToken ct = default)
        {
            var category = await _dataService.Get<FormCategory, FormCategoryDTO>(id, ct);
            if (category != null)
            {
                if (await _dataService.Context.Set<FormDefinition>().AnyAsync(definition => definition.FormCategoryId == id, ct))
                {
                    throw new BusinessException("This category has Form definitions associated. Can't be deleted.");
                }

                await _dataService.Delete<FormCategory>(id, ct);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<List<FormCategoryDTO>> GetAll(CancellationToken ct)
        {
            return (await _dataService.GetAll<FormCategory, FormCategoryDTO>(ct)).ToList();
        }
    }
}