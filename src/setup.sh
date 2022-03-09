#!/bin/bash

bash build.sh
bash pack.sh
kubectl apply -k VirtualService.Net/config/install/