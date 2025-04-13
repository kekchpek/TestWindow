#!/bin/bash

# Check if an argument was provided
if [ $# -eq 0 ]; then
    echo "Usage: $0 <number_of_images>"
    echo "Example: $0 10"
    exit 1
fi

# Get the number of images to download
n=$1

# Create a directory for the images if it doesn't exist
mkdir -p images

echo "Downloading $n images from picsum.photos..."

# Download the specified number of images
for i in $(seq 1 $n); do
    echo "Downloading image $i of $n..."
    curl -s -L -o "images/puzzle_$i.jpg" "https://picsum.photos/150/100.jpg"
    
    # Add a small delay to avoid overwhelming the server
    sleep 0.5
done

echo "Download complete! $n images saved to the 'images' directory." 