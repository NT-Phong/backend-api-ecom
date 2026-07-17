using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Common;
using Ecom.Domain.Entities;
using Ecom.Domain.Models;

namespace Ecom.Application.Common.Extensions;

/// <summary>
/// Extension methods for mapping entities to DTOs that inherit from BaseDto.
/// Automatically maps audit fields (Id, No, CreatedAt, Creator, Editor, etc.)
/// </summary>
public static class BaseDtoMapperExtensions
{

    /// <summary>
    /// Map a BaseEntity to a BaseDto and automatically fetch Creator/Editor info from database.
    /// </summary>
    public static async Task<TDto> MapToBaseDtoAsync<TDto>(
        this BaseEntity entity,
        IUnitOfWork unitOfWork,
        Action<TDto>? additionalMapping = null)
        where TDto : BaseDto, new()
    {
        var dto = entity.MapToBaseDto(additionalMapping);
        await dto.MapCreatorAsync(unitOfWork);
        return dto;
    }

    /// <summary>
    /// Map a list of BaseEntities to BaseDtos and automatically fetch Creator/Editor info from database.
    /// </summary>
    public static async Task<List<TDto>> MapToBaseDtoListAsync
        <TEntity, TDto>(
        this IEnumerable<TEntity> entities,
        IUnitOfWork unitOfWork,
        Action<TEntity, TDto>? additionalMapping = null)
        where TEntity : BaseEntity
        where TDto : BaseDto, new()
    {
        var dtos = entities.MapToBaseDtoList<TEntity, TDto>(additionalMapping);
        await dtos.MapCreatorsAsync(unitOfWork);
        return dtos;
    }

    /// <summary>
    /// Map a BaseEntity to a BaseDto, filling common audit fields.
    /// </summary>
    public static TDto MapToBaseDto<TDto>(this BaseEntity entity, Action<TDto>? additionalMapping = null)
        where TDto : BaseDto, new()
    {
        var dto = new TDto
        {
            Id = entity.Id,
            No = (ulong)entity.No,
            CreatedAt = entity.CreatedAt,
            EditedAt = entity.UpdatedAt ?? entity.CreatedAt,
            CreatorId = entity.CreatedBy,
            EditorId = entity.UpdatedBy,
        };
        additionalMapping?.Invoke(dto);
        return dto;
    }

    /// <summary>
    /// Map a list of BaseEntities to BaseDtos.
    /// </summary>
    public static List<TDto> MapToBaseDtoList<TEntity, TDto>(
        this IEnumerable<TEntity> entities,
        Action<TEntity, TDto>? additionalMapping = null)
        where TEntity : BaseEntity
        where TDto : BaseDto, new()
    {
        return entities.Select(entity =>
        {
            var dto = entity.MapToBaseDto<TDto>();
            additionalMapping?.Invoke(entity, dto);
            return dto;
        }).ToList();
    }

    /// <summary>
    /// Map Creator and Editor info for a single DTO using IUnitOfWork.
    /// </summary>
    public static async Task<TDto> MapCreatorAsync<TDto>(this TDto dto, IUnitOfWork unitOfWork) where TDto : BaseDto
    {
        if (dto == null) return dto;
        var list = new List<TDto> { dto };
        await MapCreatorsAsync(list, unitOfWork);
        return dto;
    }

    /// <summary>
    /// Map Creator and Editor info for a list of DTOs using IUnitOfWork.
    /// Queries Users and Documents from database.
    /// </summary>
    public static async Task<List<TDto>> MapCreatorsAsync<TDto>(this List<TDto> dtos, IUnitOfWork unitOfWork) where TDto : BaseDto
    {
        if (dtos == null || !dtos.Any()) return new List<TDto>();

        // 1. Lấy tất cả các CreatorId và EditorId từ danh sách DTO
        var allIds = dtos.SelectMany(d => new[] { d.CreatorId, d.EditorId })
            .Where(id => id.HasValue && id != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct().ToList();

        if (!allIds.Any()) return dtos;

        // 2. Query Users (Chỉ Select Id và FullName để tối ưu tốc độ)
        var userInfos = await unitOfWork.Repository<User>().QueryNoTracking()
            .Where(u => allIds.Contains(u.Id))
            .Select(u => new AccountCreator
            {
                Id = u.Id,
                Fullname = u.FullName ?? "N/A"
            })
            .ToListAsync();

        // 3. Gán thông tin Creator và Editor cho từng DTO
        foreach (var dto in dtos)
        {
            if (dto.CreatorId.HasValue)
                dto.Creator = userInfos.FirstOrDefault(x => x.Id == dto.CreatorId);

            if (dto.EditorId.HasValue)
                dto.Editor = userInfos.FirstOrDefault(x => x.Id == dto.EditorId);
        }

        return dtos;
    }
}
