using System;
using System.Collections.Generic;
using AutoMapper;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Maps planning poker domain entities to planning poker service data entities.
    /// </summary>
    internal static class ServiceEntityMapper
    {
        private static readonly Lazy<IConfigurationProvider> Configuration = new Lazy<IConfigurationProvider>(CreateMapperConfiguration);
        private static readonly Lazy<IMapper> MappingEngine = new Lazy<IMapper>(() => new Mapper(Configuration.Value));

        /// <summary>
        /// Maps the specified source entity to destination entity.
        /// </summary>
        /// <typeparam name="TSource">The type of the source entity.</typeparam>
        /// <typeparam name="TDestination">The type of the destination entity.</typeparam>
        /// <param name="source">The source entity to map.</param>
        /// <returns>The mapped destination entity.</returns>
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return MappingEngine.Value.Map<TSource, TDestination>(source);
        }

        private static IConfigurationProvider CreateMapperConfiguration()
        {
            MapperConfiguration result = new MapperConfiguration(config =>
            {
                config.AllowNullCollections = true;
                config.CreateMap<D.ScrumTeam, ScrumTeam>();
                config.CreateMap<D.Observer, TeamMember>()
                    .ForMember(m => m.Type, mc => mc.MapFrom((s, d, m) => s.GetType().Name));
                config.CreateMap<D.Message, Message>()
                    .Include<D.MemberMessage, MemberMessage>()
                    .Include<D.EstimateResultMessage, EstimateResultMessage>()
                    .ForMember(m => m.Type, mc => mc.MapFrom(m => m.MessageType));
                config.CreateMap<D.MemberMessage, MemberMessage>();
                config.CreateMap<D.EstimateResultMessage, EstimateResultMessage>();
                config.CreateMap<KeyValuePair<D.Member, D.Estimate>, EstimateResultItem>()
                    .ForMember(i => i.Member, mc => mc.MapFrom(p => p.Key))
                    .ForMember(i => i.Estimate, mc => mc.MapFrom(p => p.Value));
                config.CreateMap<D.EstimateParticipantStatus, EstimateParticipantStatus>();
                config.CreateMap<D.Estimate, Estimate>()
                    .ForMember(e => e.Value, mc => mc.MapFrom((s, d, m) => MapEstimateValue(s.Value)));
            });

            result.AssertConfigurationIsValid();
            return result;
        }

        private static double? MapEstimateValue(double? value) =>
            value.HasValue && double.IsPositiveInfinity(value.Value) ? Estimate.PositiveInfinity : value;
    }
}
