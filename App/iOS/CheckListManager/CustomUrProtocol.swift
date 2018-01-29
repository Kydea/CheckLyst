//
//  CustomUrlConnection.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 20/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

var requestCounts = 0

class CustomUrlprotocol: URLProtocol, URLSessionDataDelegate, URLSessionTaskDelegate {
    
    fileprivate var dataTask:URLSessionDataTask?
    fileprivate var urlResponse:URLResponse?
    fileprivate var receivedData:NSMutableData?

    class var CustomKey:String {
        return "myCustomKey"
    }

    // MARK: NSURLProtocol

    override class func canInit(with request: URLRequest) -> Bool {
//        print("Request #\(requestCounts): URL = \(request.url!.absoluteString)")
//        requestCounts += 1
        if (URLProtocol.property(forKey: CustomUrlprotocol.CustomKey, in: request) != nil) {
            return false
        }

        return true
    }

    override class func canonicalRequest(for request: URLRequest) -> URLRequest {
        return request
    }

    override func startLoading() {

        let newRequest = (self.request as NSURLRequest).mutableCopy() as! NSMutableURLRequest
        
        if (UserDefaults.standard.value(forKey: "access_token") != nil && (UserDefaults.standard.value(forKey: "logged") as! Bool)/* && newRequest.url!.absoluteString != "http://api.clm.matematici-aws.com/AccessControl/Access/Token"*/) {
            newRequest.addValue((UserDefaults.standard.value(forKey: "token_type") as! String).capitalized + " " + (UserDefaults.standard.value(forKey: "access_token") as! String), forHTTPHeaderField: "Authorization")
        }
        
        if(newRequest.url!.absoluteString.contains("http://clm.matematici-aws.com/Instances/List")) {
            (UIApplication.shared as! MyApplication).goBackToList()
        }

        URLProtocol.setProperty("true", forKey: CustomUrlprotocol.CustomKey, in: newRequest)

        let defaultConfigObj = URLSessionConfiguration.default
        let defaultSession = Foundation.URLSession(configuration: defaultConfigObj, delegate: self, delegateQueue: nil)

        self.dataTask = defaultSession.dataTask(with: newRequest as URLRequest)
        self.dataTask!.resume()

    }

    override func stopLoading() {
        self.dataTask?.cancel()
        self.dataTask       = nil
        self.receivedData   = nil
        self.urlResponse    = nil
    }

    // MARK: NSURLSessionDataDelegate

    func urlSession(_ session: URLSession, dataTask: URLSessionDataTask,
                    didReceive response: URLResponse,
                                       completionHandler: @escaping (URLSession.ResponseDisposition) -> Void) {

        self.client?.urlProtocol(self, didReceive: response, cacheStoragePolicy: .notAllowed)

        self.urlResponse = response
        self.receivedData = NSMutableData()

        completionHandler(.allow)
    }

    func urlSession(_ session: URLSession, dataTask: URLSessionDataTask, didReceive data: Data) {
        self.client?.urlProtocol(self, didLoad: data)
        self.receivedData?.append(data)
    }

    // MARK: NSURLSessionTaskDelegate

    func urlSession(_ session: URLSession, task: URLSessionTask, didCompleteWithError error: Error?) {
        if error != nil && (error! as NSError).code != NSURLErrorCancelled {
            self.client?.urlProtocol(self, didFailWithError: error!)
        } else {
            //saveCachedResponse()
            self.client?.urlProtocolDidFinishLoading(self)
        }
    }


    func saveCachedResponse () {
        
        //let urlString = self.request.URL?.absoluteString
        // let dataString = NSString(data: self.receivedData!, encoding: NSUTF8StringEncoding) as NSString?
        //print("URL: \(urlString)\n\nDATA:\(dataString)\n\n")
    }

    
    
}
