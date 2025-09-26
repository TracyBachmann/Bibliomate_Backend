using AutoMapper;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Configuration
{
    /// <summary>
    /// Defines AutoMapper configuration profiles for mapping 
    /// between domain entities and Data Transfer Objects (DTOs).
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfile"/> class
        /// and configures all mappings used in the application.
        /// </summary>
        public MappingProfile()
        {
            // Configure mapping from Loan entity to LoanReadDto
            CreateMap<Loan, LoanReadDto>()
                // Maps the UserName property in LoanReadDto.
                // Since User is mandatory, no null-check is needed.
                // Concatenates FirstName and LastName, trims extra spaces.
                .ForMember(
                    dest => dest.UserName,
                    opt => opt.MapFrom(src =>
                        ($"{src.User.FirstName} {src.User.LastName}").Trim()))

                // Maps the BookTitle property in LoanReadDto.
                // Since Book is mandatory, no null-check is needed.
                .ForMember(
                    dest => dest.BookTitle,
                    opt => opt.MapFrom(src => src.Book.Title));
        }
    }
}