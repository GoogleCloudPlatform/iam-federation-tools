# Deploy AI Agent Authenticator

This article describes how you can deploy the AI Agent Authenticator (AAAuth) by using Terraform.

???+ info "Before you begin"

    To follow the instructions in this guide, you need the following:

    *   [ ] Google Cloud project to deploy the application in. We recommend creating a dedicated project
            for deploying AAAuth.

    If you're using Gemini Enterprise 
    [with workforce identity federation and Microsoft Entra :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/configure-identity-provider),
    you also need:

    *   [ ] Permission to modify the Entra App registration used by workforce identity federation.

Deploying AAAuth requires the following billable components of Google Cloud:

- [Cloud Run](https://cloud.google.com/run/pricing)
- [Artifact Registry](https://cloud.google.com/artifact-registry/pricing)


## Prepare the deployment

To prepare the deployment, do the following:

1.  Select or create a Google Cloud project to deploy the application in. We recommend creating
    a dedicated project for deploying AAAuth.
 
    [Open Project selector](https://pantheon.corp.google.com/projectselector2/home/dashboard){ .md-button }

1.  Enable billing for your project. 
 
    [Open Billing](https://support.google.com/cloud/answer/6293499#enable-billing){ .md-button }

1.  Open Cloud Shell.

    [Open Cloud Shell](https://console.cloud.google.com/?cloudshell=true){.md-button}

1.  Set an environment variable to contain your
    [project ID](https://cloud.google.com/resource-manager/docs/creating-managing-projects):

        export PROJECT_ID=project-id

    Replace `project-id` with the ID of your project.

1.  Set another environment variable to contain your preferred region:

        export REGION=region
    
    Replace `region` with a region that supports Cloud Run and Compute Engine,
    for example `us-central1`.

1.  Authorize `gcloud`:

        gcloud auth login

    You can skip this step if you're using Cloud Shell.

1.  Authorize `terraform`:

        gcloud auth application-default login &&
        gcloud auth application-default set-quota-project $PROJECT_ID

    You can skip this step if you're using Cloud Shell.

1.  Clone the AAAuth Git repository and switch to the latest branch: 
 
        git clone https://github.com/GoogleCloudPlatform/iam-federation-tools.git
        cd iam-federation-tools/aaauth
        git checkout latest 
    
You're now ready to deploy AAAuth to Cloud Run.

## Deploy AAAuth to Cloud Run

To deploy AAAuth to Cloud Run by using Terraform, do the following:

1.  Change to the `terraform` directory:

        cd aaauth/terraform

1.  Create a file named `terraform.tfvars` and configure it depending on the
    [identity provider :octicons-link-external-16:](https://docs.cloud.google.com/gemini/enterprise/docs/configure-identity-provider)
    you use for Gemini Enterprise:

    === "Google Identity"

            cat << EOF > terraform.tfvars
            project_id = "$PROJECT_ID"
            region = "$REGION"
            EOF

    === "Entra"

            cat << EOF > terraform.tfvars
            project_id = "$PROJECT_ID"
            region = "$REGION"
            entra_tenant = "TENANT"
            entra_provider = "PROVIDER"
            EOF

        Open the file `terraform.tfvars` and replace the following:

        +   `TENANT`: the ID of your Entra tenant, in the format
            `00000000-0000-0000-0000-000000000000`.
        +   `PROVIDER`: resource name of your workforce identity provider, in the format
            `locations/global/workforcePools/POOL/providers/PROVIDER`.

            This must be the same workforce identity provider as the one you use for
            Gemini Enterprise.


1.  Set up authentication for Artifact Registry:

        gcloud auth configure-docker $REGION-docker.pkg.dev
    
1.  Initialize Terraform:

        terraform init
    

1.  Apply the configuration:

        terraform apply -var-file=terraform.tfvars
    
    When the deployment completes, terraform outputs the URL of the Cloud Run service.
    Note down this URL, you'll need it later.

AAAuth is now deployed, but you still need to configure your identity provider.

## Configure your identity provider

Configure your identity provider so that AAAuth can authenticate users. The steps 
differ depending on the identity provider you use for Gemini Enterprise:

=== "Google Identity"

    If you've configured Gemini Enterprise to use Google Identity, you
    must create an OAuth consent screen and client ID:

    1.  In the Cloud Console, go to **APIs & Services > Credentials**.
    1.  Click **Configure consent screen > Get started** and configure the following settings:

        *   **App name**: Enter a name such as `AI Agent Authenticator`.
        *   **User support email**: Select an email address.
        *   **Audience**: **Internal**.
        *   **Contact Information**: Enter an email address.

        Then click **Create**.

    1.  Click **Create OAuth client**
    1.  Select **Web application** and configure the following settings:

        *   **Name**: Enter a name such as `AI Agent Authenticator`.
        *   **Authorized redirect URIs**: 
        
                https://SERVICE/google-identity/continue

            Replace `SERVICE` with the domain name of the Cloud Run service -- for example, `service-xxxxxx-as.a.run.app`.
    1.  Click **Create**.

        Note down the client ID and the client secret, you'll need it later.

    1.  Click **OK**.

=== "Entra"

    If you've configured Gemini Enterprise to use workforce identity federation
    with Microsoft Entra, you must update the App registration used by your workforce 
    identity provider. 

    1.  In the Entra admin center, open the App registration used by your workforce 
        identity provider. 
    1.  Go to **Authentication**.
    1.  Click **Add redirect URI**.
    1.  Select **Web** and enter the following URI: 
    
            https://SERVICE/entra-delegated/continue

        Replace `SERVICE` with the domain name of the Cloud Run service -- for example, `service-xxxxxx-as.a.run.app`.

    1.  Click **Configure**.
    1.  Go to **Certificates and secrets**.
    1.  Click **New client secret** and add an additional secret.

        Note down the secret, you need it when you [configure Gemini Enterprise](aaauth-configure-gemini-enterprise.md). 
    

## What's next

[Configure Gemini Enterprise](aaauth-configure-gemini-enterprise.md) so that it can use AAAuth.