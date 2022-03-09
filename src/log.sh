#!/bin/bash

kubectl logs -l operator=vs-conf-operator -n vs-conf-system -f