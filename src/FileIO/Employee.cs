using System;

namespace FileIO
{
    /// <summary>
    /// Employee Record
    /// </summary>
    public struct Employee 
    {
        /// <summary>
        /// Name of the Employee
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Email of Employee
        /// </summary>
        public string Email { get; set; }
        public DateTime DateOfJoining { get; set; }
        public double Salary { get; set; }
        public int Age { get; set; }
        
        
    }
}