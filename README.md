# virtual-service-dotnet

An operator to deal with the priority of istio virtual service by watching the virtual service config `crd`.

## Prerequest

- Kubernetes 1.20+ `required`
 
- Istio 1.12+ `required`, and only tested in 1.12.1, should be ok in higher version

- Docker `required`

- .Net 6 `optional` if you want to develop your own features

- Visual Studio 2022 `optional` if you want to generate the crds or installation yamls
