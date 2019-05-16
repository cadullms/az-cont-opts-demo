### Use ACR to build our images

https://docs.microsoft.com/en-us/learn/modules/build-and-store-container-images
https://github.com/Azure-Samples/acr-tasks
https://docs.microsoft.com/en-us/azure/container-registry/container-registry-tasks-reference-yaml
https://docs.microsoft.com/en-us/azure/container-registry/container-registry-tutorial-base-image-update
https://blogs.msdn.microsoft.com/stevelasker/2017/12/20/os-framework-patching-with-docker-containers-paradigm-shift/
https://stevelasker.blog/2018/03/01/docker-tagging-best-practices-for-tagging-and-versioning-docker-images/

az group create -n cadullacr -l westeurope
az acr create -g cadullacr -n cadullacr --sku Basic
az acr update -n cadullacr --admin-enabled true
az acr credential show -g cadullacr -n cadullacr

in folder ui:

az acr build --registry cadullacr --image azcontoptions:v0.2 .

show in the portal, show "run instance" and "deploy in web app" in context menus, explain that this needs admin login enabled

### Use ACI to run a web image



### x. Use ACI to run a background task

az provider list --query "[?contains(namespace,'Microsoft.ContainerInstance')]" -o table
az provider register --namespace Microsoft.ContainerInstance

### x. Use Virtual Kubelet to scale AKS with ACI

Follow steps in [Create and configure an Azure Kubernetes Services (AKS) cluster to use virtual nodes using the Azure CLI](https://docs.microsoft.com/en-us/azure/aks/virtual-nodes-cli)

But...


And...

show the taints and toleration thing:
```sh
kubectl get node virtual-node-aci-linux
kubectl get node virtual-node-aci-linux -o=jsonpath='{.spec.taints}'
```
