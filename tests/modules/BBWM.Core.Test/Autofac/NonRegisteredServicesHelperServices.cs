namespace BBWM.Core.Test.Autofac;

public interface IMatchingNameMultipleDirectInterfacesService
{
}

public interface ISomeRandomInterface
{
}

public class MatchingNameMultipleDirectInterfacesService
    : IMatchingNameMultipleDirectInterfacesService,
      ISomeRandomInterface
{
}

public interface IUnmatchedNameSingleDirectInterfaceService
{
}

public class UnmatchedNameSingleImplementationTypoService : IUnmatchedNameSingleDirectInterfaceService
{
}

public class IWrongTypeService
{
}

public interface WrongServiceName
{
}

public interface INoImplementorsService
{
}

public interface IMultipleImplementorsService
{
}

public class MultipleImplementorOneService : IMultipleImplementorsService
{
}

public class MultipleImplementorTwoService : IMultipleImplementorsService
{
}

public interface IIndirectImplementationService
{
}

public interface IIntermediateService : IIndirectImplementationService
{
}

public class IndirectImplementationService : IIntermediateService
{
}

public interface IInvalidNameMultipleDirectInterfacesService
{
}

public class InvalidNameMultipleDirectInterfacesTypoService
    : IInvalidNameMultipleDirectInterfacesService,
      ISomeRandomInterface
{
}
