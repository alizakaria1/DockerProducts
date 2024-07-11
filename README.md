this is an application which can be completely run on docker. It includes MSSQL, redis for caching, nginx as a load balancer and reactjs for the frontend.
In this application you can upload a product with image, view it and delete it. All you need to do is run the docker compose file you can find in the repository.
Most of the images used are custom and uploaded on docker hub.

alizakaria/products:v2 : this is the image for the backend api.

alizakaria/productsui:v5 : this is an image of react for the frontend

alizakaria/productsdb:v5 : this is an image created on top of the mssql image by adding a command which creates a database when the container starts

alizakaria/nginxproduct:v1 : this is an image created on top of nginx image by adding an nginx.conf file to map the routes of the api

both frontend and backend contain a workflow created by github actions to automate the creation and deployment of the image to docker hub.

you can find the frontend github link : https://github.com/alizakaria1/react-docker
