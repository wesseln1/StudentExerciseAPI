﻿using System;
using System.Collections.Generic;
using System.Text;

namespace StudentExerciseAPI.Models
{
    class Instructor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SlackHandle { get; set; }
        public string Speciality { get; set; }
        public int CohortId { get; set; }
        public string CohortName { get; set; }
    }
}
