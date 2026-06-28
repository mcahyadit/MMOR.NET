#!/usr/bin/python3
import os
from os.path import dirname
from pathlib import Path
import socketserver
import sys
import http.server

PORT = int(sys.argv[1] if len(sys.argv) > 1 else 8000)
DIRECTORY = dirname(Path(__file__).parent.resolve())
os.chdir(DIRECTORY)

class NoCacheHandler(http.server.SimpleHTTPRequestHandler):
  def end_headers(self):
    self.send_header(
      "Cache-Control", "no-store, no-cache, must-revalidate, max-age=0"
    )
    self.send_header("Pragma", "no-cache")
    self.send_header("Expires", "0")
    super().end_headers()

  def log_message(self, format, *args):
    print(f"[serve] {self.address_string()} - {format % args}", flush=True)

with socketserver.TCPServer(("0.0.0.0", PORT), NoCacheHandler) as httpd:
    print(f"Serving at http://localhost:{PORT}")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nServer stopped.")
