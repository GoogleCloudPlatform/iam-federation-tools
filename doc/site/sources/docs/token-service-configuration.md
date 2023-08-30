# Configuration

You can customize the behavior of the Token Service application by editing the `env` section of the
[Cloud Run configuration file :octicons-link-external-16:](https://cloud.google.com/run/docs/reference/yaml/v1)

The Token Service application supports the following environment variables:

<table>
  <tr>
    <th>Name</th>
    <th>Description</th>
    <th>Required</th>
    <th>Default</th>
    <th>Available since</th>
  </tr>
  <tr>
    <td colspan="5"><b>Basic configuration</b><br>
    These options are required for the application to work.</td>
  </tr>
  <tr>
    <td>
        <code>AUTH_FLOWS</code>
    </td>
    <td>
        <p>
            A comma-separated list of authentication flows to enable.
            The following flows are supported:
            <ul>
                <li><code>xlb-mtls-client-credentials</code>: mTLS
            </ul>
        </p>
    </td>
    <td>Required</td>
    <td>(None)</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>WORKLOAD_IDENITY_PROJECT_NUMBER</code>
    </td>
    <td>
        <p>
            The Project number of the project that contains the workload identity pool.
        </p>
    </td>
    <td>Required</td>
    <td>(None)</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>WORKLOAD_IDENITY_POOL_ID</code>
    </td>
    <td>
        <p>
            The workload identity pool ID.
        </p>
    </td>
    <td>Required</td>
    <td>(None)</td>
    <td>1.0</td>
  </tr>
  <tr>
    <td>
        <code>WORKLOAD_IDENITY_PROVIDER_ID</code>
    </td>
    <td>
        <p>
            The workload identity provider ID.
        </p>
    </td>
    <td>Required</td>
    <td>(None)</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>TOKEN_VALIDITY</code>
    </td>
    <td>
        <p>
            The duration (in minutes) for which ID tokens remain valid.
        </p>
    </td>
    <td>Required</td>
    <td>5</td>
    <td>1.0</td>
  </tr> 
  
  
  <tr>
    <td colspan="5"><b>mTLS configuration</b><br>
    Use these options if you've <a href='https://cloud.google.com/load-balancing/docs/https/custom-headers-global'>
    customized the names of headers used by the load balancer</a>.
    </td>
  </tr> 
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_ID</code>
    </td>
    <td>
        The name of HTTP header that contains the client ID.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-Spiffe</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_PRESENT</code>
    </td>
    <td>
        The name of HTTP header that determines whether a certificate was present.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-Present</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_CHAIN_VERIFIED</code>
    </td>
    <td>
        The name of HTTP header that determines whether the certificate chain has been verified.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-Chain-Verified</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_ERROR</code>
    </td>
    <td>
        The name of HTTP header that contains error information.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-Error</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_SHA256_FINGERPRINT</code>
    </td>
    <td>
        The name of HTTP header that contains the SHA256 certificate fingerprint.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-Hash</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_SPIFFE_ID</code>
    </td>
    <td>
        The name of HTTP header that contains the Spiffe ID.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-Spiffe</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_URI_SANS</code>
    </td>
    <td>
        The name of HTTP header that contains URI Subject Alternative Names.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-URI-SANs</td>
    <td>1.0</td>
  </tr> 
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_DNSNAME_SANS</code>
    </td>
    <td>
        The name of HTTP header that contains DNS Subject Alternative Names.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-DNSName-SANs</td>
    <td>1.0</td>
  </tr>  
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_SERIAL_NUMBER</code>
    </td>
    <td>
        The name of HTTP header that contains the certificate serial number.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-Serial-Number</td>
    <td>1.0</td>
  </tr>  
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_VALID_NOT_BEFORE</code>
    </td>
    <td>
        The name of HTTP header that contains the not-before date for the certificate.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-Valid-Not-Before</td>
    <td>1.0</td>
  </tr>  
  <tr>
    <td>
        <code>MTLS_HEADER_CLIENT_CERT_VALID_NOT_AFTER</code>
    </td>
    <td>
        The name of HTTP header that contains the not-after date for the certificate.
    </td>
    <td>Required</td>
    <td>X-Client-Cert-Valid-Not-After</td>
    <td>1.0</td>
  </tr> 
  
  <tr>
    <td colspan="5"><b>Advanced</b></td>
  </tr>
  <tr>
    <td>
        <code>TOKEN_ISSUER</code>
    </td>
    <td>
        <p>
            Custom issuer to use in ID tokens.
        </p>
    </td>
    <td>Required</td>
    <td>Determined automatically</td>
    <td>1.0</td>
  </tr> 
</table>
