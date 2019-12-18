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
    public class CohortController : Controller
    {
        // GET: /<controller>/

        private readonly IConfiguration _config;

        public CohortController(IConfiguration config)
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
                    cmd.CommandText = @"SELECT c.Id as CohortId, [Name] as CohortName, s.Id as StudentId, s.FirstName as StudentFirstName, s.LastName as StudentLastName, s.SlackHandle as StudentSlack, s.CohortId as StudentCohortId, i.Id as InstructorId, i.FirstName as InstructorFirstName, i.LastName as InstructorLastName, i.SlackHandle as InstructorSlack, Speciality, i.CohortId as InstructorCohortId FROM Cohort c LEFT JOIN Student s ON s.CohortId = CohortId LEFT JOIN Instructor i ON i.CohortId = c.Id";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Cohort> allCohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        var cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        var cohortAlreadyAdded = allCohorts.FirstOrDefault(c => c.Id == cohortId);

                        if (cohortAlreadyAdded == null)
                        {

                            Cohort aCohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortID")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };

                            var studentIsInCohort = !reader.IsDBNull(reader.GetOrdinal("StudentId"));

                            if (studentIsInCohort)
                            {
                                Student aStudent = new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId"))
                                };

                                aCohort.Students.Add(aStudent);
                            }

                            var isInCohort = !reader.IsDBNull(reader.GetOrdinal("InstructorId"));

                            if (isInCohort)
                            {
                                Instructor aInstructor = new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                    Speciality = reader.GetString(reader.GetOrdinal("Speciality")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                                };

                                aCohort.Instructors.Add(aInstructor);
                            }


                            allCohorts.Add(aCohort);

                        }
                    }
                    reader.Close();

                    return Ok(allCohorts);
                }
            }
        }
    }
}
