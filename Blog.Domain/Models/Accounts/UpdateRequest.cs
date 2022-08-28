namespace Blog.Domain.Models.Accounts;

using Blog.Domain.Enums;
using System.ComponentModel.DataAnnotations;


public class UpdateRequest
{

    public string Title { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

}