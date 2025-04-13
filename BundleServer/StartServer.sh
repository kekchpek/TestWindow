#!/bin/bash

# Default port
PORT=8000

# Start HTTP server using Python 3
echo "Starting HTTP server at http://localhost:$PORT"
python3 -m http.server "$PORT"
