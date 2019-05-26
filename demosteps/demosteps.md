### Prep

Create Storage queue

```sh
az group create -n cadullq -l westeurope
az storage account create -n cadullq -g cadullq --sku Standard_LRS
az storage queue create --account-name cadullq --name cadullqazu 
```

### Use ACR to build our images

* https://docs.microsoft.com/en-us/learn/modules/build-and-store-container-images
* https://github.com/Azure-Samples/acr-tasks
* https://docs.microsoft.com/en-us/azure/container-registry/container-registry-tutorial-base-image-update
* https://blogs.msdn.microsoft.com/stevelasker/2017/12/20/os-framework-patching-with-docker-containers-paradigm-shift/
* https://docs.microsoft.com/en-us/azure/container-registry/container-registry-tasks-reference-yaml
* https://stevelasker.blog/2018/03/01/docker-tagging-best-practices-for-tagging-and-versioning-docker-images/

```sh
az group create -n cadullacr -l westeurope
az acr create -g cadullacr -n cadullacr --sku Basic
az acr update -n cadullacr --admin-enabled true
az acr credential show -g cadullacr -n cadullacr
```

In folder ui:

```sh
az acr build --registry cadullacr --image azcontoptionsui:v0.4 .
```

* Show in the portal, show "run instance" and "deploy in web app" in context menus, explain that this needs admin login enabled.
* Explain patching with acr tasks.

In folder worker:

```sh
az acr build --registry cadullacr --image azcontoptionsworker:v0.4 .
```

### Use ACI to run a web image

Use context menu in one of the tags in ACR.

### Use App Service to run a web image

Use context menu in one of the tags in ACR again. Then show how much more web specific features we have for Web App for Containers than for ACI. Mention as well that getting the logs out of the container is much harder here.

### Use Virtual Kubelet to scale AKS with ACI

Follow steps in [Create and configure an Azure Kubernetes Services (AKS) cluster to use virtual nodes using the Azure CLI](https://docs.microsoft.com/en-us/azure/aks/virtual-nodes-cli)

And then...

show the taints and toleration thing:
```sh
kubectl get node virtual-node-aci-linux
kubectl get node virtual-node-aci-linux -o=jsonpath='{.spec.taints}'
```

[Create Secret using kubectl](https://kubernetes.io/docs/tasks/configure-pod-container/pull-image-private-registry/#create-a-secret-by-providing-credentials-on-the-command-line)

```sh
kubectl create secret docker-registry regcred --docker-server=cadullacr.azurecr.io --docker-username=cadullacr --docker-password=<your-pword> --docker-email=cadullacr@azurecr.io
```

and then use it in yaml as described [here](https://kubernetes.io/docs/tasks/configure-pod-container/pull-image-private-registry/#create-a-pod-that-uses-your-secret).

```sh
kubectl apply -f .\factorial-worker.yaml
```
### Enable Dashboard

kubectl create clusterrolebinding kubernetes-dashboard --clusterrole=cluster-admin --serviceaccount=kube-system:kubernetes-dashboard
az aks browse -n cadullvk -g cadullvk