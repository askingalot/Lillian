#!/bin/bash
antlr4 Lillian.g4 -o java/
cd java
javac *.java
grun Lillian program -tokens ../code.LLL
cd ..