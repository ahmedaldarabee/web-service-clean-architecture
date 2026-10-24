using Microsoft.AspNetCore.Mvc;
using StudentAPIBusinessLayer;
using StudentAPIDataAccessLayer;
using WService.Model;

namespace WService.Controllers {

    //[Route("api/[controller]")] isn't recommended to use dynamic controller !
    [Route("api/StudentAPI")]
    [ApiController]
    public class StudentController : ControllerBase {

        [HttpGet("GetAllStudent")]
        public ActionResult<IEnumerable<StudentDTO>> GetAllStudent() {
            var studentList = StudentAPIBusinessLayer.Student.GetAllStudents();

            if(studentList.Count == 0){
                return NotFound("There are no student found!");
            }

            return Ok(studentList);
        }

        [HttpGet("GetPassedStudent")]
        public ActionResult<IEnumerable<Model.Student>> GetPassedStudent() {
            var passedStudent = StudentAPIBusinessLayer.Student.GetPassedStudent();
            if (passedStudent.Count == 0) {
                return NotFound("There are no student found!");
            }
            return Ok(passedStudent);
        }

        [HttpGet("GetAVGStudent")]
        public ActionResult<double> GetAVGStudent() {
            var result = StudentAPIBusinessLayer.Student.GetStudentAverage();
            if (result <= 0) {
                return NotFound("no student founded to calculate average");
            }
            return Ok(result);
        }

        [HttpGet("GetStudentById")]
        public ActionResult<IEnumerable<Model.Student>> GetStudentById(int id) {
            if (id < 1) {
                return BadRequest("enter corrected id");
            }
            var studentList = StudentAPIBusinessLayer.Student.GetStudentByID(id);
            if (studentList == null) {
                return NotFound("no student founded");
            }

            return Ok(studentList);
        }

        [HttpPost("AddStudent")]
        public ActionResult<Model.Student> AddStudent(StudentDTO student) {
            try {
                if (student == null)
                {
                    return BadRequest("Please submit data to enable insert it!");
                }

                int newStudentId = StudentAPIBusinessLayer.Student.AddStudent(student);

                return CreatedAtAction(nameof(GetStudentById),new { id = newStudentId },student);
            }
            catch (Exception ex) {
                return StatusCode(500, $"An unexpected error occurred while insertiong the student. {ex.Message}");
            }
        }

        [HttpDelete("DeleteStudentById")]
        public IActionResult DeleteStudentById(int Id) {
            try {
                if(Id < 0) {
                    return BadRequest("Opps, please send correct student id");
                }

                var student = StudentAPIBusinessLayer.Student.DeleteStudent(Id);

                if(student == false){
                    return NotFound("Opps, this student doesn't exist!");
                }

                return Ok(new { message = "Student deleted successfully"});
            }
            catch(Exception ex){
                //Next Section FIX Error: Not all code paths return a value
                return StatusCode(500, $"An unexpected error occurred while deleting the student. {ex.Message}");
            }
        }

        [HttpPatch("UpdateStudent")]
        public ActionResult<Model.Student> UpdateStudentName(StudentDTO student) {
            try {
                if (student == null || student.Id < 1) {
                    return BadRequest("Please submit data to enable insert it!");
                }

                bool result = StudentAPIBusinessLayer.Student.UpdateStudent(student);
                
                if(!result){
                    return NotFound("Something went wrong about updating this student");
                }
                
                return Ok(new { message = "Student updated successfully", updatedStudent = student });
            }
            catch (Exception ex) {
                return StatusCode(500, $"An unexpected error occurred while updated the student. {ex.Message}");
            }
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UpdateImage(IFormFile file){
            try {

                if (file == null || file.Length == 0){
                    return BadRequest("no file sent!");
                }

                var fileLocation = @"C:\Users\ahmed\Desktop\General\Imgs";
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(fileLocation, fileName); 
                
                if (!Directory.Exists(fileLocation)){
                    Directory.CreateDirectory(fileLocation);
                }

                using (var steam = new FileStream(filePath, FileMode.Create)){
                    await file.CopyToAsync(steam); // here it will store file into imageSrc
                }

                return Ok(new { filePath });
            }

            catch(Exception ex){
                return StatusCode(500, $"An unexpected error occurred while uploading image. {ex.Message}");
            }
        }

    }
}
