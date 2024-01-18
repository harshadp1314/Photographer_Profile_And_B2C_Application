using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace UploadMusic.Models
{
    public class UsersRoleProvider : RoleProvider
    {
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            string st = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            List<string> arr = new List<string>();
            Utility.Utility u = new Utility.Utility();
            try
            {
                using (SqlConnection con = new SqlConnection(st))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select RollName from logindetails l join UserRolesMapping u on l.registrationid=u.userID join RoleMaster r on u.RoleID=r.ID where Email='"+username+"'", con))
                    {
                        //cmd.Parameters.AddWithValue("@Id", Id);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                              
                                arr.Add(Convert.ToString(rd["RollName"]));


                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                string Exception = ex.Message;
            }
            //using (EmployeeDBContext context = new EmployeeDBContext())
            //{
            //    var userRoles = (from user in context.Users
            //                     join roleMapping in context.UserRolesMappings
            //                     on user.ID equals roleMapping.UserID
            //                     join role in context.RoleMasters
            //                     on roleMapping.RoleID equals role.ID
            //                     where user.UserName == username
            //                     select role.RollName).ToArray();
            //    return userRoles;
            //}
            return arr.ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}