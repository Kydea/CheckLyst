//
//  serverConnection.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 23/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

open class ServerConnection {
    
    
    static var baseUrl : String = (Server.getServer((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)?.serverUrl)!
    

    static func bindListWithParams(_ params : Dictionary<String, Dictionary<String, String?>>, completion: ((_ result:ServerResponse?) -> Void)!){
        
        ServerConnection.sendHTTPRequest(baseUrl+"/Instances/List/Bind", params: params as Dictionary<String, AnyObject>){ (result) -> Void in
        completion(result)
        }
    }
    
    static func getListInstancesWithParams(_ params : Dictionary<String, Any>, completion: ((_ result:ServerResponse?) -> Void)!){
        
        ServerConnection.sendHTTPRequest(baseUrl+"/Instances/List/GetData", params: params as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    static func getCompaniesRequest(_ completion: ((_ result:ServerResponse?) -> Void)!){
        
        ServerConnection.sendHTTPRequest(baseUrl+"/Infrastructure/Companies/GetData", params: [:] as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    static func getAppInfo(_ completion: ((_ result:ServerResponse?) -> Void)!){
        
        ServerConnection.sendHTTPRequest(baseUrl+"/Infrastructure/Information/Get", params: [:] as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    static func sendGetClientRequest(_ params : Dictionary<String, Dictionary<String, String>>, completion: ((_ result:ServerResponse) -> Void)!){
        sendHTTPRequest(baseUrl+"/AccessControl/Users/GetData", params: params as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }

    
    static func loginOrRefreshToServer(_ params : Dictionary<String, String>, completion: ((_ result:ServerResponse) -> Void)!){
        sendHTTPRequest(baseUrl+"/AccessControl/Access/Token", params: params as Dictionary<String, AnyObject>){ (result) -> Void in
                completion(result)
        }
    }


    static func logoutFromServer(_ params : Dictionary<String, Dictionary<String, String>>,_ completion: ((_ result:ServerResponse) -> Void)!){
        
        sendHTTPRequest(baseUrl+"/AccessControl/Access/Logout", params: params as Dictionary<String, AnyObject>){ (result) -> Void in
            UserDefaults.standard.setValue(false, forKey: "logged")
            UserDefaults.standard.setValue(nil, forKey: "access_token")
            UserDefaults.standard.setValue(nil, forKey: "refresh_token")
            completion(result)
        }
    }
    

    static func closeInstance(_ params : Dictionary<String, String>, completion: ((_ result:ServerResponse) -> Void)!){
        sendHTTPRequest(baseUrl+"/CheckList/Close", params: params as Dictionary<String, AnyObject>){ (result) -> Void in
                completion(result)
        }
    }

    static func sendClockRequest(_ params : Dictionary<String, AnyObject>, completion: ((_ result:ServerResponse) -> Void)!) {
        
        sendHTTPRequest(baseUrl+"/Server/Clock", params: params){ (result) -> Void in
            completion(result)
        }
    }

    
    static func sendHTTPRequest(_ urlPath: String,params : Dictionary<String, AnyObject>, completion: ((_ result:ServerResponse) -> Void)!){
        
        do {
            
            let request = NSMutableURLRequest(url: URL(string: urlPath)!)
            
            let session = URLSession.shared
            
            request.httpMethod = "POST"
            
            request.timeoutInterval = 20
            
            //print("params\(params.debugDescription)");
            
            if (!params.isEmpty)
                
            {
                if((params["Password"]) != nil) {
                    request.addValue("application/x-www-form-urlencoded", forHTTPHeaderField: "Content-Type")
                    
                    let bodyString = "grant_type=password" + "&companyId=" + (params["Company"] as! String) + "&username=" + (params["Code"] as! String) + "&password=" + (params["Password"] as! String)
                    
                    //print(bodyString);
                    
                    request.httpBody = bodyString.data(using: String.Encoding.utf8)
                    
                } else if((params["Refresh_Token"]) != nil) {
                    
                    request.addValue("application/x-www-form-urlencoded", forHTTPHeaderField: "Content-Type")
                    
                     let bodyString = "grant_type=refresh_token&refresh_token=" + (params["Refresh_Token"] as! String)
                    
                    //print(bodyString);
                    
                    request.httpBody = bodyString.data(using: String.Encoding.utf8)
                    
                } else {
                    request.addValue("application/json", forHTTPHeaderField: "Content-Type")
                    
                    //print("json result \(String.init(data: try JSONSerialization.data(withJSONObject: params, options:[]), encoding: String.Encoding.utf8) ?? "no json")")
                    
                    request.httpBody = try  JSONSerialization.data(withJSONObject: params, options:[])
                }
                
            }
            
            request.addValue("application/json", forHTTPHeaderField: "Accept")
            
            let task = session.dataTask(with: request as URLRequest, completionHandler: { (data, response, error) in
                
                let priority = DispatchQoS.default.qosClass
                DispatchQueue.global(qos: priority).async {
                    DispatchQueue.main.async {
                        
                        if((error as NSError?)?.code == -1202)
                        {
                            UserDefaults.standard.setValue("true", forKey: "invalidCertificate")
                        }
                        
                        else
                        {
                            UserDefaults.standard.setValue("false", forKey: "invalidCertificate")
                        }
                        
                        let serverResponse = ServerResponse()
                        serverResponse.response = response
                        serverResponse.data = data
                        serverResponse.error = error as NSError?
                        
                        /*print("response = \(String(describing: serverResponse.response))");
                        print("data = \(String(describing: serverResponse.data))")
                        print(String(data: serverResponse.data!, encoding: .utf8)!)
                        print(serverResponse.error ?? "nessun errore");*/
                        
                        completion(serverResponse)
                    }
                }
            })
            
            task.resume()
        }
        catch{  print("error")  }
        
    }
    

    
    static func sendHTTPGetRequest(_ urlPath: String, completion: ((_ result:ServerResponse) -> Void)!){
       
        do{
            let request = NSMutableURLRequest(url: URL(string: urlPath)!)
            let session = URLSession.shared
            request.httpMethod = "GET"
            request.timeoutInterval = 20
            
            let task = session.dataTask(with: request as URLRequest, completionHandler: { (data, response, error) in
                let priority = DispatchQoS.default.qosClass
                DispatchQueue.global(qos: priority).async {
                    DispatchQueue.main.async    {
                    
                    let serverResponse = ServerResponse()
                    serverResponse.response = response
                    serverResponse.data = data
                    serverResponse.error = error as NSError?
                    completion(serverResponse)
                    }
                }
            })
            task.resume()
        }
    }
}
