using Application.Interfaces;
using Domain.Entities; 
using Application.DTO;
using Application.Interfaces;

namespace  Application.Services.Persons
{
    public class PersonService : IPersonService
    { 
     private readonly IPerson _person;
        public PersonService(IPerson person)
        {
            _person = person; 
        }
        public List<Person> GetAllPersons()
        {
            return _person.GetAllPersons();                     
        }
        public Person GetPersonById(int Id)
        {
         return _person.GetPersonById(Id);
        }
       public void CreatePerson(PersonCreateDTO personDTO)
        {
           _person.CreatePerson( personDTO);
        }
        public void UpdatePerson(int Id, PersonUpdateDTO personDTO)
         {
             _person.UpdatePerson(Id, personDTO);
         }
    }
}