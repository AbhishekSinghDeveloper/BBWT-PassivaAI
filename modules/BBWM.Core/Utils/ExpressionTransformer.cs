using AutoMapper;
using AutoMapper.Internal;

using System.Linq.Expressions;

namespace BBWM.Core.Utils;


// possible replacement: see https://docs.automapper.org/en/stable/Expression-Translation-(UseAsDataSource).html
public static class ExpressionTransformer
{
    private class ParameterTypeVisitor<TSource, TDestination> : ExpressionVisitor
    {
        private readonly Dictionary<string, ParameterExpression> _parameters;
        private readonly Expression<Func<TSource, bool>> _expression;
        private readonly IMapper mapper;

        public ParameterTypeVisitor(Expression<Func<TSource, bool>> expression, IMapper mapper)
        {
            _parameters = expression.Parameters
                .ToDictionary(p => p.Name, p => Expression.Parameter(typeof(TDestination), p.Name));

            _expression = expression;
            this.mapper = mapper;
        }

        public Expression<Func<TDestination, bool>> Transform()
        {
            return (Expression<Func<TDestination, bool>>)Visit(_expression);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            TypeMap typeMap = mapper.ConfigurationProvider.Internal().GetAllTypeMaps().FirstOrDefault(m => m.SourceType == node.Member.DeclaringType);
            if (typeMap is not null)
            {
                var memberName = node.Member.Name;
                var member = typeMap.PropertyMaps.FirstOrDefault(p => p.SourceMember.Name == memberName);
                if (member is not null)
                {
                    var expression = Visit(node.Expression);
                    return Expression.MakeMemberAccess(expression, member.DestinationMember);
                }
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var parameter = _parameters[node.Name];
            return parameter;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var expression = Visit(node.Body);
            return Expression.Lambda(expression, _parameters.Select(x => x.Value));
        }
    }

    public static Expression<Func<TDestination, bool>> Tranform<TSource, TDestination>(Expression<Func<TSource, bool>> sourceExpression, IMapper mapper)
    {
        var map = mapper.ConfigurationProvider.Internal().FindTypeMapFor<TSource, TDestination>();
        if (map is null)
        {
            throw new AutoMapperMappingException(string.Format("No Mapping found for {0} --> {1}.", typeof(TSource).Name, typeof(TDestination).Name));
        }
        var visitor = new ParameterTypeVisitor<TSource, TDestination>(sourceExpression, mapper);
        return visitor.Transform();
    }
}
