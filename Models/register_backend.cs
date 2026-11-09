using Microsoft.Data.SqlClient; 

namespace RaceDayAPI.Models
{
    public class register_backend
    {
        //start of register_backend class

        //creating a method to store user's information
        //in the database using SqlConnection and SqlCommand
        //called participants

        public string store_user(string register_fullname, string register_email, string role, string register_password)
        {//start of store_user method

            //variable declaration to store the connection string to the database
            string message = "Failed to register user, try again later...";
            string connectionString = @"Data source=(localdb)\part_2_api;database=raceday2;";

            //try and catch
            try
            {//start of try statement

                //get the password and hash it using BCrypt.Net.BCrypt.HashPassword method
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(register_password);

                //creating an instance for the SqlConnection class
                //with an object name called connect
                SqlConnection connect = new SqlConnection(connectionString);

                //open the connection to the database
                connect.Open();

                //query to insert the user's information into the database
                string query = @"insert into participants (fullname, email, role, password) values ('" + register_fullname + "', '" + register_email + "', '" + role + "', '" + hashedPassword + "')";

                //creating an instance for the SqlCommand class
                //with an object name called command
                SqlCommand run_query = new SqlCommand(query, connect);

                //run/execute the query to insert the user's information into the database
                //run_query.ExecuteNonQuery();
                int results_affected = run_query.ExecuteNonQuery();

                //check if the query was successful and return a message
                if (results_affected > 0)
                {//start of if statement

                    message = "User registered successfully";

                }//end of if statement
                else
                {//start of else statement

                    message = "Failed to register user, try again later...";

                }//end of else statement

                //close the connection to the database
                connect.Close();

            }// end of try statement
            catch (Exception error)
            {//start of catch statement

                //return error message
                //assign first
                message = error.Message;

            }//end of catch statement

            //return message
            return message;
        }//end of store_user method

    }
}
