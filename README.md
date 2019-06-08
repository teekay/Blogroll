# About
---

Running a self-published blog on a platform that does not have a blogroll component? You can run this web app to administer your links, then import them to your weblog as an HTML snippet. Or, if you are feeling fancy, import them as JSON then present them in whichever way you'd like.

## Platform
ASP.NET Core 2.1 using either SQLite or LiteDB for data storage. Host it on Windows, Linux, in the cloud or in a Docker container.

## License

MIT. Do what you want with this and do not sue me if it does not work.

## Installation
### Docker
Review the Dockerfile and customize as necessary. Then deploy to your favorite cloud provider. For my own blog, I am using Azure with a Linux host.

### VPS
Publish the project "Blogroll.Web", which contains the web app. I strongly suggest putting the Kestrel behind a proxy such as `nginx`. Then, you'd
want to obtain a SSL certificate, configure your web server accordingly. By default, the app does not require or care about SSL but you do.

## Configuration
There are three environment variables you want to set:
- `Auth_Admin__Password` to be set with a password you'll use to manage your links (no username)
- `Data_Data__Engine` to be set with either `sqlite` or `litedb` (case-insensitive)
- `Data_Data__Storage` to be set with the relative or absolute path to an existing folder where the app will store the database file.

## Administration
Head over to the URL of your deployed app and log in using the pre-configured password (hopefully, over SSL).

For each link you want to add, the only truly mandatory field is the "Blog address". Enter a name if you want to customize it. If not, the app will try to obtain the name from the website's RSS feed.

Likewise, the "Feed url" field is optional. The app will perform auto-discovery if not entered.

You can change the ordering of the links within your blogroll by dragging and dropping the links.

## Integration
There's a simple anonymous API that serves your links as text, HTML, or JSON.

Assuming your deployed instance runs at `https://blogroll.example.com`, it's API server runs at `https://blogroll.example.com/api/v1`.

There are two endpoints:

- `/api/v1/links` for links that include a snippet  from the link's RSS feed's latest entry
- `/api/v1/links/simple` for links that do not include said snippet

By default, the endpoint will return HTML. You can explicitly request the content type by either specifying it in the request header, or by affixing `.json`, `.txt`, or `.html` to the URL, e.g. `https://blogroll.example.com/api/v1/links/simple.json` for JSON output.

### Example of integration
Edit your blog's template and place the following HTML snippet where you want your blogroll to appear:
```
<aside class="blogroll-host" id="blogroll-host" style="display:none">
  <h3>Blogroll</h3>
  <div id="blogroll-contents">
  </div>
</aside>
```

Then, add this at the bottom of your template:
```
<script>
(function(){  
  var request = new XMLHttpRequest();
  request.open('GET', 'https://blogroll.example.com/api/v1/links.html', true);
  request.onload = function () {
    if (request.status >= 200 && request.status < 400) {
      // Success!        
      var home = document.getElementById("blogroll-contents");
      if (home != null) {
        home.innerHTML = request.responseText;
      }
      document.getElementById("blogroll-host").style.display="block";
    } else {
      // We reached our target server, but it returned an error
      console.log("Could not fetch the links");
    }
  };

  request.onerror = function (err) {
    // There was a connection error of some sort
    console.log(err);
  };

  request.send();
})();
</script>
```
(replacing `https://blogroll.example.com/api/v1/links.html` with the URL containing your instance's domain name).

For this to work, set up your web server such that it sends the right CORS headers like this:
`Access-Control-Allow-Origin: https://amazing.site` where `https://amazing.site` will be the URL where your blog is published.

You will notice it takes a second or two for your blogroll to appear. I will address caching and performance topics later. In the meantime, if you want top performance today, save the generated HTML and simply put it straight in your template, bypassing the XHR request altogether.

## For developers and technical audience
This is my attempt to examine how to write truly object-oriented code with C#. The source of my inspiration were talks and books by [Yegor Bugayenko](<https://www.yegor256.com/>) - "Elegant objects".

On a high level, this meant:

- objects encapsulate *something*, and don't give it away freely -> no public properties whatsoever
- objects are generally responsible for one thing; their agenda is limited
- functionality is extended using composition not inheritance

There were two areas where I struggled before finding an implementation pattern I could tentatively accept.

### Presenting objects in the real world

A typical app will want to show something to the user. To that end, objects routinely make their properties available to the UI layer via public properties that can be easily accessed in templates.

The downside of properties is that they enable disrespectful, procedural way of programming. The object then acts only as a data structure that anybody can molest. E.g.:

```
if (user.Age > 25)
{
// do something
}
else 
{
// do something else
}
```
If I need anything done based on the user's age, I should enable the object to do the work on its own. I don't need to know its internal structure and should only call its methods to achieve my goals.

How can I then present the object if I don't know its presentable properties?

Given a hint from Yegor - "printers, not getters" - I enabled my objects to "print themselves" to various format - plain text, JSON, HTML - using the `IMedia` interface implementations.

The template designer still needs to know which properties are addressable in the templates, these properties are however unavailable to the programmer. So, the object might have a `Name` property that the designer can place somewhere but the programmer cannot "get" `myUser.Name`.

### Persisting objects

There's a similar story when it comes to dealing with databases.

The fastest route to hell is to use an ORM that forces you to deal with DTOs and similar "naked data" containers. These will have public properties with public getters AND setters. Gross.

The alternative is to teach my objects to speak SQL and I have attempted to do that. Currently, though, there's still data leakage from my object to the composing class that does the actual persistence. This I will attempt to fix shortly.

### Next steps
I want to address caching, performance, and keep an eye on refactorings that would make the code even more OOP in spirit.
