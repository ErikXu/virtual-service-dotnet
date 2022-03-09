# virtual-service-dotnet

An operator to deal with the priority of istio virtual service by watching the virtual service config `crd`.

## Prerequest

- Kubernetes 1.20+ is `required`
 
- Istio 1.12+ is `required`, and only tested in 1.12.1, should be ok in higher version

- Docker is `required`

- .Net 6 is `optional` if you want to develop your own features

- Visual Studio 2022 is `optional` if you want to generate the crd or installation yamls

## Installation

- Build and pack

  - Enter the [src](src) directory
  
  - Using `bash build.sh` to build the application
  
  - Using `bash pack.sh` to pack the docker image
  
  If you want to push the image to your docker registry, please modify the [pack.sh](src/pack.sh) and using your registry address before running `bash pack.sh`.

- Install crd and operator

  - Enter the [config](src/VirtualService.Net/config) directory

  - With your cluster `kubeconfig`, running `kubectl apply -k install/` to install crd and operator

  If you are using your own docker registry, please modify the image info of [deployment.yaml](src/VirtualService.Net/config/operator/deployment.yaml) before running `kubectl apply -k install/`.

## Examples

- Enter the [example](example) directory

- Using `kubectl apply -f xxx.yaml` to easily start an example

- Using `kubectl get vs xxx -o yaml` to see the generated virtual service

- Using `kubectl get vsc` to see the virtual service config(s)

- Using `kubectl delete -f xxx.yaml` to cleanup the example

## Cleanup

You could enter the [src](src) directory and use [cleanup.sh](src/cleanup.sh) to cleanup the crd and operator.
