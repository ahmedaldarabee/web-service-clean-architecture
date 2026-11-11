using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WService.DataSimulation;
using WService.Model;

namespace WService.Controllers {


    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentInMemoryController : ControllerBase {

        [HttpGet("GetAllStudents")]
        public ActionResult<IEnumerable<Student>> GetAllStudents(){
            if (StudentDataSimulation.studentList.Count == 0) {
                return NotFound("No Students Found!");
            }
            return Ok(StudentDataSimulation.studentList);
        }

        [HttpGet("GetStudentById")]
        public async Task<ActionResult<Student>> GetStudentById(
            int id,
            [FromServices] IAuthorizationService authService            
            ){
            if (id < 1){
                return BadRequest("Enter corrected identifier");
            }

            if (StudentDataSimulation.studentList.Count == 0){
                return NotFound("No Students Found!");
            }

            var student = StudentDataSimulation.studentList.FirstOrDefault(std => std.Id == id);

            if (student==null){
                return NotFound("No Students Found!");
            }

            // [ Step 5 ] 
            var authenticatedUser = await authService.AuthorizeAsync(User,id, "StudentOwner");

            if (!authenticatedUser.Succeeded){
                return Forbid();
            }

            return Ok(student);
        }


        [HttpGet("GetPassedStudents")]
        public ActionResult<IEnumerable<Student>> GetPassedStudents(){
            var passedStudents = StudentDataSimulation.studentList.Where(student => student.Grade >= 50).ToList();
            if (passedStudents.Count == 0){
                return NotFound("No Students Passed");
            }
            return Ok(passedStudents);
        }


        [HttpPost("AddStudent")]
        public ActionResult<Student> AddStudent(Student newStudent) {            
            if (newStudent == null || string.IsNullOrEmpty(newStudent.Name)){
                return BadRequest("Invalid student data.");
            }

            newStudent.Id = StudentDataSimulation.studentList.Count > 0 ? StudentDataSimulation.studentList.Max(s => s.Id) + 1 : 1;
            StudentDataSimulation.studentList.Add(newStudent);

            return CreatedAtRoute("GetStudentById", new { id = newStudent.Id }, newStudent);
        }

        [HttpDelete("DeleteStudentById")]
        public ActionResult DeleteStudent(int id){
            if (id < 1){
                return BadRequest($"Not accepted ID {id}");
            }

            var student = StudentDataSimulation.studentList.FirstOrDefault(s => s.Id == id);
            
            if (student == null){
                return NotFound($"Student with ID {id} not found.");
            }

            StudentDataSimulation.studentList.Remove(student);
            return Ok($"Student with ID {id} has been deleted.");
        }

        [HttpPut("{id}", Name = "UpdateStudent")]
        public ActionResult<Student> UpdateStudent(int id, Student updatedStudent) {
            if (id < 1 || updatedStudent == null || string.IsNullOrEmpty(updatedStudent.Name)){
                return BadRequest("Invalid student data.");
            }

            var student = StudentDataSimulation.studentList.FirstOrDefault(s => s.Id == id);
            if (student == null){
                return NotFound($"Student with ID {id} not found.");
            }

            student.Name = updatedStudent.Name;
            student.Age = updatedStudent.Age;
            student.Grade = updatedStudent.Grade;

            return Ok(student);
        }
    }
}
