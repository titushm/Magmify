package main

import (
	"io"
	"net/http"
	"os"
)

func main() {
	args := os.Args[1:]
	appDirectory := args[0]
	appDirectory = appDirectory // to avoid unused variable error

	req, err := http.NewRequest("GET", "https://api.github.com/repos/titushm/updater_template/releases/latest", nil)
	if err != nil {
		panic(err)
	}
	req.Header.Set("User-Agent", "magmifyupdater/1.0")
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()

	// Read and print the JSON response
	body, err := io.ReadAll(resp.Body)
	if err != nil {
		panic(err)
	}
	println(string(body))
}
