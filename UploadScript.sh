#!/usr/bin/env bash

set -o nounset
set -o errexit
set -o pipefail

declare -r max=4
declare i=0

function wrap() {
  mono_version=$(mono --version | awk '/version/ { print $5 }')
  if [ "$TRAVIS_OS_NAME" = "linux" ] ;
    then
	echo "Linux worker"
	if [ "$mono_version" != "3.2.8" ] ;
		then
		echo "Wrong Mono version- Not uploading this build"
		exit
	else
		echo "Mono version OK- Uploading build"
	fi
	else
	echo "Mac worker- Uploading this build"
  fi
  local cmd=$1 ; shift
  retry $cmd "$@"
  local success=$?
  set -o errexit
  exit $success
}

function retry() {
  set +o errexit
  local cmd=$1 ; shift
  $cmd "$@"
  s=$?
  if [ $s -ne 6 ] ;
    then
    return $s
  elif [ $s -ne 0 -a $i -lt $max ] ;
  then
    i=$(($i+1))
    echo "Retrying"
    sleep $((1+$i*$i*5))
    retry $cmd "$@";
  else
    return $s
  fi
}

wrap "$@"
