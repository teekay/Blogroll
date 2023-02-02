# About

---

Running a self-published blog on a platform that does not have a blogroll component? You can run this web app to administer your links, then import them to your weblog as an HTML snippet. Or, if you are feeling fancy, import them as JSON then present them in whichever way you'd like.

**IMPORTANT** - this is a toy/learning project. I'm using it as I prepare for my AZ-204 exam.

## Platform

ASP.NET Core 6 using SQLite, LiteDB, Azure Tables, or MySQL for data storage.

Host it on Windows, Linux, in the cloud or in a Docker container.

## License

MIT. Do what you want with this and do not sue me if it does not work.

## Installation

### Docker
Review the Dockerfile and customize it as necessary. Then deploy to your favorite cloud provider. For my blog, I am using Azure with a Linux host.

### VPS / Azure App Service

Publish the project `Blogroll.Web`, which contains the web app. I strongly suggest putting the Kestrel behind a proxy such as `nginx`. Then, you'd want to obtain an SSL certificate, and configure your web server accordingly. By default, the app does not require or care about SSL but you do.

### Azure Functions

Deploy the project `Blogroll.Api` as a function app in Azure.

## Configuration

There are three environment variables you want to set:

- `Auth_Admin__Password` to be set with a password you'll use to manage your links (no username)
- `Data_Data__Engine` to be set with `sqlite`, `litedb`, `azuretables`, or `mysql` (case-insensitive)
- `Data_Data__Storage` - when using `sqlite` or `litedb`, set this with the relative or absolute path to an existing folder where the app will store the database file.

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

```javascript
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
