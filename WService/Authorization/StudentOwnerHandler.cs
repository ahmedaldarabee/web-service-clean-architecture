using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WService.Authorization {

    //in backstage:
    //ASP.NET That response about Fill AuthorizationHandlerContext with information and pass it to the guard on your behalf.

    //AuthorizationHandlerContext [ Main ]: that content all of requirement: [ JWT Token , Sender Data / Resource like user id ]

    //step 1 [ Requirements / Policy Side ] - It does not contain any data, does not know who the user info.
    //it will just define info about requirement required from user to allow a it to enter.

    //step 2 [ Guard ]
    //StudentOwnerHandler that be as : Guard, and we call the guard functionality by doing call to IAuthorizationService as:
    // await authService.AuthorizeAsync .... [ In any wanted controller ]

    //The guard roles [ check if this student is the owner or not? ] - so it will do: 
    //- Inspection about incoming user by owned token and claims
    //- Inspection incoming user with our logic if be matched or not THEN
    //- it will return the response [ Decision Making ]

    //Final Result - we answer this question: Does this user have permission to view this item or not?

    //AuthorizationHandler<StudentOwnerRequirement,int>: 
    //that ASP.NET Core provided to us to use to create our own guardian.
    public class StudentOwnerHandler : AuthorizationHandler<StudentOwnerRequirement,int>{
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentOwnerRequirement requirement, int studentId){

            if (context.User.IsInRole("Admin")){
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userId,out int authUserId) && authUserId == studentId){
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

//How it define sender user id here and how that be know matched with exist our info for specific student?            
//Answer: The server it will extract user info from JWT Token that send by user in header side 

//var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//var userRole = User.FindFirstValue(ClaimTypes.Role);

//var authUserId = int.Parse(userId!);

//bool isAdmin = userRole == "Admin";
//if (!isAdmin && authUserId != id){
//    return Forbid();
//}
