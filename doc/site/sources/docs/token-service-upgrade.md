# Upgrade the Token Service

To change the configuration of the Token Service or to upgrade to a newer version, 
do the following:

1.  In the Google Cloud console, switch to your project and then open Cloud Shell. 
 
    [Open Cloud Shell](https://console.cloud.google.com/?cloudshell=true&){ .md-button }
    
2.  In Cloud Shell, select the region and project that contains the existing Cloud Run deployment: 
 
        gcloud config set project PROJECT_ID 
        gcloud config set run/region REGION 

3.  Clone the GitHub repository and switch to the latest branch: 
 
        git clone https://github.com/GoogleCloudPlatform/iam-federation-tools.git
        cd iam-federation-tools/token-service
        git checkout latest 

4.  Download the configuration file that you used previously to deploy the application 
    and save it to a file `app.yaml`: 
 
        gcloud run services describe token-service --format yaml > app.yaml 

5.  If you want to make changes to your configuration, edit the app.yaml file. For a list of settings, 
    see [Configuration](token-service-configuration.md). 

6.  Deploy the application: 
 
        PROJECT_ID=$(gcloud config get-value core/project)

        docker build -t gcr.io/$PROJECT_ID/token-broker:latest .
        docker push gcr.io/$PROJECT_ID/token-broker:latest

        IMAGE=$(docker inspect --format='{{index .RepoDigests 0}}'  gcr.io/$PROJECT_ID/token-broker)
        sed -i "s|image:.*|image: $IMAGE|g" app.yaml

        gcloud run services replace app.yaml


## What's next

*   Read [how to authenticate clients using the token service](token-service-authenticate-clients-mtls.md)
*   Protect the token service using 
    [Google Cloud Armor security policies :octicons-link-external-16:](https://cloud.google.com/armor/docs/configure-security-policies)
    