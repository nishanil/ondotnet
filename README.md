# ondotnet
Demos used in On.NET show

Watch https://youtu.be/zW4INO353Xg for details.


## Running the sample locally

Navigate to the `microservices` folder in a CLI. Then run the command `tye run`. If you're new to Tye, checkout the [getting started](https://github.com/dotnet/tye/blob/master/docs/getting_started.md) guide first.

## Managing resiliency in code 

Open [microservices/frontend/Startup.cs](https://github.com/nishanil/ondotnet/blob/main/microservices/frontend/Startup.cs) and uncomment the following code from the `ConfigureServices` method:

```csharp
// // uncomment the code below for handling partial failures in code
//  .AddPolicyHandler(GetRetryPolicy())
//  .AddPolicyHandler(GetCircuitBreakerPolicy());
// 
```
Run the sample again using `tye run`.


## Managing resiliency using Linkerd Service Mesh

First, deploy the application and its dependencies to Kubernetes before the services can be meshed.

### Deploying dependencies

<b>Redis</b>

Run the following command from CLI:

 ```
 kubectl apply -f https://raw.githubusercontent.com/dotnet/tye/master/docs/tutorials/hello-tye/redis.yaml
 ```

<b>Zipkin</b>

Run the following command from CLI:

 ```
kubectl apply -f https://raw.githubusercontent.com/dotnet/tye/master/docs/recipes/zipkin.yaml
 ```

Check the status using the command `kubectl get po`
```
➜  microservices git:(main) ✗ kubectl get po
NAME                     READY   STATUS    RESTARTS   AGE
redis-57455cbdbf-8czhg   1/1     Running   0          15s
zipkin-6cf9b865d-h6s4v   1/1     Running   0          5s
```

 ### Deploying application

 >Note: Do not uncomment the code shown earlier. The resiliency will be handled in the service mesh.

Run the following command from the CLI and provide your dockerhub/acr details for `Container Registry`. 

```
tye deploy -i
```

If you're new to tye, checkout the guide [Getting started with Deployment](https://github.com/dotnet/tye/blob/master/docs/tutorials/hello-tye/01_deploy.md)

Check the status using the command `kubectl get po`

```
➜  microservices git:(main) ✗ kubectl get po
NAME                       READY   STATUS    RESTARTS   AGE
backend-86b455fcc9-njj5p   2/2     Running   0          5m7s
frontend-df8546cb7-dg4xd   2/2     Running   0          5m7s
redis-57455cbdbf-8czhg     1/1     Running   0          9m21s
zipkin-6cf9b865d-h6s4v     1/1     Running   0          9m11s
```

Test the deployment using the following command:

```
kubectl port-forward svc/frontend 3000:80
```

Open, [http://localhost:3000/](http://localhost:3000/) and checkout the page. Refresh a couple of times to see the error pop up randomly.

 ### Adding Service Mesh (Linkerd)

 Download and install the [Linkerd CLI](https://github.com/linkerd/linkerd2/releases/) from the release page and add it to the `PATH`.

 #### Validate your Kubernetes cluster

 ```
 linkerd check --pre
 ```
 On success, you should get a result like this:

 ```
 ➜  microservices git:(main) ✗ linkerd check --pre
kubernetes-api
--------------
√ can initialize the client
√ can query the Kubernetes API

kubernetes-version
------------------
√ is running the minimum Kubernetes API version
√ is running the minimum kubectl version

pre-kubernetes-setup
--------------------
√ control plane namespace does not already exist
√ can create non-namespaced resources
√ can create ServiceAccounts
√ can create Services
√ can create Deployments
√ can create CronJobs
√ can create ConfigMaps
√ can create Secrets
√ can read Secrets
√ can read extension-apiserver-authentication configmap
√ no clock skew detected

pre-kubernetes-capability
-------------------------
√ has NET_ADMIN capability
√ has NET_RAW capability

linkerd-version
---------------
√ can determine the latest version
‼ cli is up-to-date
    is running version 20.12.1 but the latest edge version is 20.12.3
    see https://linkerd.io/checks/#l5d-version-cli for hints

 ```

 #### Install Linkerd onto the Kubernetes cluster

Run the following command to install Linkerd onto the cluster:

```
linkerd install | kubectl apply -f -
```
and verify with the following command:

```
kubectl -n linkerd get deploy
```

You'll see a response like this:

```
➜  microservices git:(main) ✗ kubectl -n linkerd get deploy
NAME                     READY   UP-TO-DATE   AVAILABLE   AGE
linkerd-controller       1/1     1            1           2m48s
linkerd-destination      1/1     1            1           2m48s
linkerd-grafana          1/1     1            1           2m47s
linkerd-identity         1/1     1            1           2m48s
linkerd-prometheus       1/1     1            1           2m47s
linkerd-proxy-injector   1/1     1            1           2m47s
linkerd-sp-validator     1/1     1            1           2m47s
linkerd-tap              1/1     1            1           2m47s
```
Run the following command to see the linkerd dashboard

```
linkerd dashboard &
```

 #### Add services to the Mesh

 Linkerd service mesh is an opt-in feature. You can configure the services that you like to add to the mesh. Since our services were deployed using `Tye`, we didn't have to work with Kubernetes deployment files. Hence we will use the following commands to extract the deployments and inject Linkerd annotations and redploy them onto the cluster.

Run the following commands one by one:

 ```
kubectl get deploy frontend -o yaml | linkerd inject - | kubectl apply -f -

kubectl get deploy backend -o yaml | linkerd inject - | kubectl apply -f -

kubectl get deploy zipkin -o yaml | linkerd inject - | kubectl apply -f -

kubectl get deploy redis -o yaml | linkerd inject - | kubectl apply -f -
 ```

Navigate to Linkerd Dashboard, and ensure the services are meshed.


 #### Configuring Resiliency

 Navigate to `servicemesh\linkerd` folder and run the following command from the CLI:

 ```
 kubectl apply -f .\backendServiceProfile.yaml
 ```
Forward port to your frontend service and navigate to the site to see the partial failures go away.

If you wish to use Istio instead of Linkerd, the following section will guide you install Istio as the service mesh and to configure resliency.

 ### Adding Service Mesh (Istio)

 Download and install the [istioctl](https://github.com/istio/istio/releases/) from the release page and add it to the `PATH`.

 #### Validate your Kubernetes cluster

 ```
 istioctl x precheck
 ```
 On success, you should get a result like this:

```
➜  Source istioctl x precheck

Checking the cluster to make sure it is ready for Istio installation...

#1. Kubernetes-api
-----------------------
Can initialize the Kubernetes client.
Can query the Kubernetes API Server.

#2. Kubernetes-version
-----------------------
Istio is compatible with Kubernetes: v1.18.8.

#3. Istio-existence
-----------------------
Istio will be installed in the istio-system namespace.

#4. Kubernetes-setup
-----------------------
Can create necessary Kubernetes configurations: Namespace,ClusterRole,ClusterRoleBinding,CustomResourceDefinition,Role,ServiceAccount,Service,Deployments,ConfigMap.

#5. SideCar-Injector
-----------------------
This Kubernetes cluster supports automatic sidecar injection. To enable automatic sidecar injection see https://istio.io/v1.8/docs/setup/additional-setup/sidecar-injection/#deploying-an-app

-----------------------
Install Pre-Check passed! The cluster is ready for Istio installation.

```
 #### Install Istio onto the Kubernetes cluster
Run the following command to install Istio onto the cluster with `default` config profile:

```
istioctl install --set profile=default
```
On success, you should get a result like this:
```
➜  Source istioctl install --set profile=default
This will install the Istio default profile with ["Istio core" "Istiod" "Ingress gateways"] components into the cluster. Proceed? (y/N) y
Detected that your cluster does not support third party JWT authentication. Falling back to less secure first party JWT. See https://istio.io/v1.8/docs/ops/best-practices/security/#configure-third-party-service-account-tokens for details.
✔ Istio core installed
✔ Istiod installed
✔ Ingress gateways installed
✔ Installation complete
```
More about Installation Configure Profiles, check the [documentation](https://istio.io/latest/docs/setup/additional-setup/config-profiles/)

Check the installation status with the following command:

```
kubectl -n istio-system get pods
```
Resulting to:
```
➜  Source kubectl -n istio-system get pods
NAME                                    READY   STATUS    RESTARTS   AGE
istio-ingressgateway-77bcf54747-24nvh   1/1     Running   0          3m22s
istiod-66bcf5d94f-s22s7                 1/1     Running   0          3m40s
```
#### Add services to the Mesh

You can configure the services that you like to add to the mesh. Since our services were deployed using `Tye`, we didn't have to work with Kubernetes deployment files. Hence we will use the following commands to extract the deployments and inject Istio annotations and redploy them onto the cluster.

Run the following commands one by one:

 ```
kubectl get deploy backend -o yaml | istioctl kube-inject -f - | kubectl apply -f -
kubectl get deploy frontend -o yaml | istioctl kube-inject -f - | kubectl apply -f -
kubectl get deploy redis -o yaml | istioctl kube-inject -f - | kubectl apply -f -
kubectl get deploy zipkin -o yaml | istioctl kube-inject -f - | kubectl apply -f -
 ```

#### Configuring Resiliency

 Navigate to `servicemesh\istio` folder and run the following command from the CLI:

 ```
 kubectl apply -f .\backendVirtualService.yaml
 ```
 
To install a `Kiali` dashboard, along with `Prometheus`, `Grafana`, and `Jaeger`, run the following commands:

```
kubectl apply -f https://raw.githubusercontent.com/istio/istio/release-1.8/samples/addons/prometheus.yaml

kubectl apply -f https://raw.githubusercontent.com/istio/istio/release-1.8/samples/addons/grafana.yaml

kubectl apply -f https://raw.githubusercontent.com/istio/istio/release-1.8/samples/addons/jaeger.yaml

kubectl apply -f https://raw.githubusercontent.com/istio/istio/release-1.8/samples/addons/kiali.yaml
```
To run the dashboard, run these commands:

```
kubectl rollout status deployment/kiali -n istio-system

istioctl dashboard kiali
```
Forward port to your frontend service and navigate to the site to see the partial failures go away.

That's it!

 [@nishanil](https://twitter.com/nishanil)