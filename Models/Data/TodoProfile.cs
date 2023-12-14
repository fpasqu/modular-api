using AutoMapper;
using ModularApi.Models.Data.Entities;

namespace ModularApi.Models.Data
{
    public class TodoProfile : Profile
    {
        public TodoProfile()
        {
            this.CreateMap<Todo, TodoModel>().ReverseMap();
        }
    }
}
