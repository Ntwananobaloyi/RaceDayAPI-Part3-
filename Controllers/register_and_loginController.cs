using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Models;
using System.Text.RegularExpressions;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class register_and_loginController : ControllerBase
    {
        //start of register_and_login class

        //creating a list of participants options using enum
        public enum Roles
        {// start of enum

            user, organizer

        }//end of enum

        //creating a register Request EndPoint method using
        //HttpPost to register a new user and return a response
        // fulname, email, role and password
        [HttpPost("{register_fullname}&{register_email}&{role}&{register_password}")]


        public string register_user(string register_fullname, string register_email, string role, string register_password)
        {//start of register_user method


            //check if email is valid or not using regex
            if (!Regex.IsMatch(register_email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {//start of if statement
                //return error message if email is invalid
                return "Invalid email format.Please enter valid email";
            }//end of if statement

            //check if password contains a capital letter using boolean variable
            bool hasCapital = Regex.IsMatch(register_password, @"[A-Z]");

            //check if password has a small letter using boolean variable
            bool hasSmall = Regex.IsMatch(register_password, @"[a-z]");

            //check if password has a number using boolean variable
            bool hasNumber = Regex.IsMatch(register_password, @"[0-9]");

            //check if password has a special character using boolean variable
            bool hasSpecial = Regex.IsMatch(
                register_password, @"[^a-zA-Z0-9\s]");

            //check if password has at least 8 characters using boolean variable
            bool hasLength = register_password.Length >= 8;

            //validate the password using if statement and
            //return error message if password is invalid
            if (!hasCapital || !hasSmall || !hasNumber || !hasSpecial || !hasLength)
            {//start of if statement

                return "Password must contain at least 8 characters," +
                    " including a capital letter, a small letter, " +
                    "a number and a special character";

            }//end of if statement

            //creating an istance for the class register_backend
            //with an object name "store" to store the data in the backend
            register_backend store = new register_backend();

            //temp variable to store the response from the backend
            string results = store.store_user(register_fullname, register_email, role, register_password);

            //return the results 
            return results;

        }//end of register_user method

        //creating a login Request EndPoint method to allowuser to login and manage
        //using HttpPost request method, by email and password
        [HttpPost("{login_email}&{login_password}")]

        public string login_user(string login_email, string login_password)
        {//start of login_user method  

            //creating an instance for thr class login_backend
            // with an object name "login_user" to login the user
            login_backend login_user = new login_backend();

            //temp variable to collect the results from the backend
            string results = login_user.login_user(login_email, login_password, HttpContext);

            //return the results
            return results;

        }//end of login_user method
    }
}
