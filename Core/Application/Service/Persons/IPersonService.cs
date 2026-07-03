using Domain.Entities;
using Application.DTO;

namespace Application.Services.Persons
{
     public interface IPersonService
    {
       public Person GetPersonById(int Id);
        public List<Person> GetAllPersons();
       void CreatePerson(PersonCreateDTO personDTO);
       void UpdatePerson(int Id, PersonUpdateDTO personDTO);
    }
}