Running in Docker
* Install Docker Client
* Open the project in VSCode, and build the Dockerfile
* In Dockerclient, run the image. Expand the dropdown for Optional Settings
    Set Host port to 2593 to expose the docker port
    Create a volume:
      Host Path - The UO Game directory
      Container Path - /trueuo/uo_data

The Dockerfile is now injecting Environment variable into the host, which can now directly override any Server config value.
   
   *DataPath.CustomPath=/trueuo/uo_data
   maps the windows directory to a unix directory

   *Containered=1
   used to bypass the default user setup, because Console.Read wont work in a container

   *Server.Relay=127.0.0.1               
   for local use at least for now, the server relay will force pass this IP, because the docker container picks up as a private IP, and it defaults the server to the docker host. Which breaks the post authentication step.

* You can also set bind mounts to externalise the Saves and Config
    Such as:
    C:\Games\server\Config⁠	/trueuo/Config
    C:\Games\server\Saves⁠	/trueuo/Saves