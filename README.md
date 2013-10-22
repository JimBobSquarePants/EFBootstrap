# EFBootstrap

A class library written in C# for communicating with databases using the Entity Framework (built against EF6) from Microsoft. Comes with 2nd  level caching.

These classes contain generic repositories allowing you to persist any model to/from your databases. They contain methods for repositories in readonly and readwrite flavours. The readonly version will automatically provide 2nd level caching
for your queries for a rolling ten minute period (configurable) which is flushed on any database updates.

# Installation

Download a copy of the repo and build the solution then add a reference to the binaries to your solution.

I *might* add this to Nuget if there is any traction.

# Wiring it up

I only really use MVC so the instructions are written in that context however the classes should be compatible with any .NET 4 product.

So.... Lets imagine we are creating a blog engine. I won't go into any detail on generating Code First Models as there is plenty of documentation out there.

## Sessions

Create your `DbContext` file as per usual in this case we are calling it **BlogEntitiesContext**.

You'll need to create two classes, one for each repository type e.g

### ReadWrite

``` c#
/// <summary>
/// Encapsulates methods for persisting objects to and from data storage
/// using the Entity Framework.
/// </summary>
public class SiteEFSession : ReadWriteSession
{
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Blog.Models.SiteEFSession"/> class. 
    /// </summary>
    public SiteEFSession()
        : base(new BlogEntitiesContext())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Blog.Models.SiteEFSession"/> class. 
    /// </summary>
    /// <param name="context">The <see cref="T:System.Data.Entity.DbContext">DBContext</see> 
    /// for querying and working with entity data as objects.</param>
    public SiteEFSession(DbContext context)
        : base(context)
    {
    }
    #endregion
}
```

### ReadOnly

``` c#
/// <summary>
/// Encapsulates methods for retrieving objects from data storage
/// using the Entity Framework.
/// </summary>
public class SiteEFReadOnlySession : ReadOnlySession
{
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Blog.Models.SiteEFReadOnlySession"/> class. 
    /// </summary>
    public SiteEFReadOnlySession()
        : base(new BlogEntitiesContext())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Blog.Models.SiteEFReadOnlySession/> class. 
    /// </summary>
    /// <param name="context">The <see cref="T:System.Data.Entity.DbContext"/> 
    /// for querying and working with entity data as objects.</param>
    public SiteEFReadOnlySession(DbContext context)
        : base(context)
    {
    }
    #endregion
}
```
    
You will first need to wire up your controllers to use these classes. 

An example would be something like this:

```c#
/// <summary>
/// Encapulates methods required for basic controller functionality.
/// </summary>
public class BaseController : Controller
{
    #region Properties
    /// <summary>
    /// Gets or sets the session for persisting objects to and from data storage
    /// using the Entity Framework Code First.
    /// </summary>
    public SiteEFCodeFirstSession ReadWriteSession { get; set; }

    /// <summary>
    /// Gets or sets the session for retrieving objects from data storage
    /// using the Entity Framework Code First.
    /// </summary>
    public SiteEFCodeFirstReadOnlySession ReadOnlySession { get; set; }
    #endregion
        
    #region Methods
    /// <summary>
    /// Overrides the base implimentation of the base OnActionExecuting method. 
    /// Called before the action method is invoked.
    /// </summary>
    /// <param name="filterContext">
    /// A <see cref="T:System.Web.Mvc.ActionExecutingContext"/> providing information 
    /// about the current request and action.
    /// </param>
    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);
        this.ReadWriteSession = new SiteEFCodeFirstSession();
        this.ReadOnlySession = new SiteEFCodeFirstReadOnlySession();
    }

    /// <summary>
    /// Overrides the base implimentation of the base OnActionExecuting method. 
    /// Called after the action method is invoked.
    /// </summary>
    /// <param name="filterContext">
    /// A <see cref="T:System.Web.Mvc.ActionExecutingContext"/> providing information 
    /// about the current request and action.
    /// </param>
    protected override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        this.ReadWriteSession.CommitChanges();
    }
    #endregion   
}
```
    
You could, if you like, for flexibility wire it up using the interfaces `ISession` and `IReadOnlySession` and dependency injection maybe 
using something like [Ninject] (http://www.ninject.org/) This would give you much more freedom should you want to change ORM.
        
After that you should be ready to go.

## Usage

Once everything is wired up querying the database is now very easy.

Lets say I wanted to query all the posts in my blog in an ordered list. 

``` c#
    public ActionResult Index()
    {
        List<PagedPost> posts =
            this.ReadOnlySession.Any<Post>(p => p.IsDeleted == false && p.IsPublished)
                                .OrderByDescending(
									p => p.DateCreated).Select(
										p => new PagedPost
											 {
										 		Id = p.Id,
										 		Title = p.Title,
										 		Content = p.Content,
										 		RelativeLink = p.RelativeLink,
										 		AdminLink = p.AdminLink,
										 		DateCreated = p.DateCreated.ToString("d")
										 	}).ToList();

        return this.View(posts);
    }
```       
 
Or if I wanted to save a post that I've edited.

``` c#
public ActionResult SavePost(Post post)
{
    // Get the post to edit.
    Post newPost = this.ReadWriteSession.First<Post>(p => p.Id == post.Id);

    if (newPost == null)
    {
        this.ReadWriteSession.Add(post);
    }
    else
    {
        newPost.Title = post.Title;
        newPost.Content = post.Content;
        newPost.IsPublished = post.IsPublished;
                
        this.ReadWriteSession.Update(newPost);
    }
            
    // Saving changes to the database is handled in the base controller.
    return RedirectToAction("Index", "Home", new { area = string.Empty });
}
```        

Simples eh? Hopefully this should be enough to get started with but if you get stuck just ask on twitter https://twitter.com/James_M_South
