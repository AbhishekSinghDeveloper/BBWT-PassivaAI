using AutoMapper;

namespace BBWM.FileStorage;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<FileDetails, FileDetailsDTO>()
            .ForMember(dst => dst.Key, opts => opts.MapFrom(src => $"{src.Key}.{src.Extension}"))
            //[ThumbnailGUID]
            .ForMember(dst => dst.ThumbnailKey, opts => opts.MapFrom(src => $"{src.ThumbnailKey}.{src.Extension}"))
            .ForMember(dst => dst.FileName, opts => opts.MapFrom(src => $"{src.FileName}.{src.Extension}"))
            .ForMember(dst => dst.Size, opts => opts.MapFrom(src => src.SizeBytes))
            .ForMember(dst => dst.Url, opts => opts.MapFrom<FileDetailsUrlResolver, string>(src => $"{src.Key}.{src.Extension}"))
            .ForMember(dst => dst.ThumbnailUrl, opts => opts.MapFrom<FileDetailsUrlResolver, string>(src => $"{src.ThumbnailKey}.{src.Extension}"))
            .ReverseMap();
    }
}

public class FileDetailsUrlResolver : IMemberValueResolver<FileDetails, FileDetailsDTO, string, string>
{
    private readonly IFileStorageProvider _provider;

    public FileDetailsUrlResolver(IFileStorageProvider provider) => _provider = provider;

    public string Resolve(FileDetails source, FileDetailsDTO destination, string sourceMember, string destMember, ResolutionContext context) =>
        sourceMember is null ? null : _provider.GetFile(sourceMember)?.Result?.Url;
}
