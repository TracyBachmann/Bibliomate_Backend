using AutoMapper;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Loan -> LoanReadDto
            CreateMap<Loan, LoanReadDto>()
                .ForMember(d => d.UserName,
                    o => o.MapFrom(s =>
                        s.User != null
                            ? ($"{s.User.FirstName} {s.User.LastName}").Trim()
                            : string.Empty))
                .ForMember(d => d.BookTitle,
                    o => o.MapFrom(s => s.Book != null ? s.Book.Title : string.Empty));
        }
    }
}