namespace Ecom.Application.Common.Extensions;

public static class MapperExtensions
{
    public static TDto ProjectTo<TEntity, TDto>(this TEntity entity)
    {
        if (entity == null)
            return default;
        var dtoType = typeof(TDto);
        var entityType = typeof(TEntity);
        var entityProps = entityType.GetProperties();
        var dto = Activator.CreateInstance(typeof(TDto));
        foreach (var entityProp in entityProps)
        {
            var dtoProp = dtoType.GetProperty(entityProp.Name);
            if (
                dtoProp == null ||
                dtoProp.GetCustomAttribute<NoMapAttribute>() != null ||
                entityProp.GetCustomAttribute<NoMapAttribute>() != null
            )
                continue;

            var valor = entityProp.GetValue(entity, null);
            dtoType.GetProperty(entityProp.Name)?.SetValue((TDto)dto, valor);
        }

        return (TDto)dto;
    }

    public static List<TDto> ProjectTo<TEntity, TDto>(this List<TEntity> entities)
    {
        if (entities == null || !entities.Any())
            return new List<TDto>();
        var dtoType = typeof(TDto);
        var entityType = typeof(TEntity);
        var entityProps = entityType.GetProperties();
        var dtos = new List<TDto>();
        foreach (var entity in entities)
        {
            var dto = Activator.CreateInstance(typeof(TDto));
            foreach (var entityProp in entityProps)
            {
                var dtoProp = dtoType.GetProperty(entityProp.Name);
                if (
                    dtoProp == null ||
                    dtoProp.GetCustomAttribute<NoMapAttribute>() != null ||
                    entityProp.GetCustomAttribute<NoMapAttribute>() != null
                )
                    continue;

                var valor = entityProp.GetValue(entity, null);
                dtoType.GetProperty(entityProp.Name)?.SetValue((TDto)dto, valor);
            }

            dtos.Add((TDto)dto);
        }

        return dtos;
    }
}
