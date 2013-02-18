# EFBootstrap

A simple class library written in C# for communicating with databases using the Entity Framework from Microsoft.

These classes contain generic repositories allowing you to persist any class to/from your databases. They contain methods for CodeFirst and DB First repositories in readonly and readwrite flavours. The readonly versions will automatically cache your queries for a rolling one minute period which is flushed on any database updates.

# Installation

Download a copy of the repo and build the solution then add a reference to the binaries to your solution.

I *might* add this to Nuget if there is any traction.

# Wiring it up

I only really use MVC so the instructions are written in that context however the classes should be compatible with any .NET product utilising the **System.Web.Caching.Cache** class.

So.... Lets imagine we are crating a blogging engine. I won't go into any detail on generating Code First Models as there is plenty of documentation out there.

## Sessions

Create your `DbContext` file as per usual in this case we are calling it **BlogEntitiesContext**.

You'll need to create two classes, one for each repository type e.g

### ReadWrite

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

### ReadOnly

    /// <summary>
    /// Encapsulates methods for retirieving objects from data storage
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
    
You will first need to wire up your controllers to use these classes. I prefer to do it using the interfaces `ISession` and `IReadOnlySession` and dependancy injection using the awesome **Ninject** http://www.ninject.org/ This gives me much more freedom should I want to change ORM.

An example would be something like this:

    /// <summary>
    /// Encapulates methods required for basic controller functionality.
    /// </summary>
    public class BaseController : Controller
    {
        #region Fields
        /// <summary>
        /// The session for persisting objects to and from data storage.
        /// </summary>
        internal readonly ISession ReadWriteSession;

        /// <summary>
        /// The readonly session for retrieving objects from data storage.
        /// </summary>
        internal readonly IReadOnlySession ReadOnlySession;
        #endregion
        
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blog.Controllers.BaseController"/> class. 
        /// </summary>
        /// <param name="session">
        /// A <see cref="T:EFBootstrap.Interfaces.ISession"/>for persisting 
        /// objects to and from data storage.
        /// </param>
        /// <param name="readonlySession">
        /// A <see cref="T:EFBootstrap.Interfaces.IReadOnlySession"/> for retrieving objects from 
        /// data storage.
        /// </param>
        public BaseController(ISession session, IReadOnlySession readonlySession)
        {
            this.ReadWriteSession = session;
            this.ReadOnlySession = readonlySession;
        }
        #endregion      
        
    }
    
and add the following code to the generated Ninject class in App_Start

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            // Bind the data acces session interfaces to the entity framework implimentation.
            kernel.Bind<ISession>().To<SiteEFSession>().InRequestScope();
            kernel.Bind<IReadOnlySession>().To<SiteEFReadOnlySession>().InRequestScope();
        }    
        
After that you should be ready to go.

## Usage

Once everything is wired up querying the database is now very easy.

Lets say I wanted to query all the posts in my blog in an ordered list. 

        public ActionResult Index()
        {
            List<PagedPost> posts =
                this.ReadOnlySession.Any<Post>(p => p.IsDeleted == false && p.IsPublished).OrderByDescending(
                    p => p.DateCreated).Select(
                        p =>
                        new PagedPost
                            {
                                Id = p.Id,
                                Title = p.Title,
                                Content = p.Content,
                                RelativeLink = p.RelativeLink,
                                AdminLink = p.AdminLink,
                                DateCreated = p.DateCreated.ToString("d", Utils.ResolveCulture())
                            }).ToList();

            return this.View(posts);
        }
        
Or if I wanted to save a post that I've edited.

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
                
                // This line is only needed for CodeFirst and does nothing in DBFirst
                this.ReadWriteSession.Update(newPost);
            }
            
            // Save the changes to the database.
            this.ReadWriteSession.CommitChanges();

            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }
        
Simples eh? Hopefully this should be enough to get started with but if you get stuck just ask on twitter https://twitter.com/James_M_South
