using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StudentExerciseAPI.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StudentExerciseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstructorController : Controller
    {
        // GET: /<controller>/

        private readonly IConfiguration _config;

        public InstructorController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, Speciality, c.Id as CohortId, [Name] as CohortName, s.Id as StudentId, s.FirstName as StudentFirstName, s.LastName as StudentLastName, s.SlackHandle as StudentSlack, s.CohortId as StudentCohort FROM Instructor i LEFT JOIN Cohort c ON CohortId = c.Id LEFT JOIN Student s ON s.CohortId = c.Id";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Instructor> allInstructors = new List<Instructor>();

                    while (reader.Read())
                    {
                        var instructorId = reader.GetInt32(reader.GetOrdinal("Id"));
                        var instructorAlreadyAdded = allInstructors.FirstOrDefault(s => s.Id == instructorId);

                        if (instructorAlreadyAdded == null)
                        {

                            Instructor aInstructor = new Instructor
                            {
                                Id = instructorId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                            };

                            var isInCohort = !reader.IsDBNull(reader.GetOrdinal("CohortId"));

                            if (isInCohort)
                            {
                                aInstructor.Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortID")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Students = new List<Student>(),
                                    Instructors = new List<Instructor>()
                                };
                            }

                            var studentIsInCohort = !reader.IsDBNull(reader.GetOrdinal("StudentId"));

                            if(studentIsInCohort)
                            {
                                Student aStudent = new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohort"))
                                };

                                aInstructor.Cohort.Students.Add(aStudent);
                            }

                            allInstructors.Add(aInstructor);

                        }
                    }
                    reader.Close();

                    return Ok(allInstructors);
                }
            }
        }
    }
}
