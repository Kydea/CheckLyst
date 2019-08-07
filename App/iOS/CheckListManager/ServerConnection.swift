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
    
    
    static var baseUrl : String { return (Server.getServer((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)?.serverUrl)! }
    
    static var baseFrontEndUrl : String { return (Server.getServer((UIApplication.shared.delegate as! AppDelegate).managedObjectContext)?.serverFrontEndUrl)! }
    
    
    static func bindListWithParams(_ params : Dictionary<String, Dictionary<String, String?>>, completion: ((_ result:ServerResponse) -> Void)!){
        
        ServerConnection.sendHTTPRequest(baseUrl+"/Instances/List/Bind", parameters: params as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    static func getListInstancesWithParams(_ params : Dictionary<String, Any>, completion: ((_ result:ServerResponse) -> Void)!){
        
        ServerConnection.sendHTTPRequest(baseUrl+"/Instances/List/GetData", parameters: params as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    static func getCompaniesRequest(_ completion: ((_ result:ServerResponse) -> Void)!){
        
        ServerConnection.sendHTTPRequest(baseUrl+"/Infrastructure/Companies/GetData", parameters: [:] as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    static func getAppInfo(_ completion: ((_ result:ServerResponse) -> Void)!){
        
        ServerConnection.sendHTTPRequest(baseUrl+"/Infrastructure/Information/Get", parameters: [:] as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    static func sendGetClientRequest(_ params : Dictionary<String, Dictionary<String, String>>, completion: ((_ result:ServerResponse) -> Void)!){
        sendHTTPRequest(baseUrl+"/AccessControl/Users/GetData", parameters: params as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    
    static func loginOrRefreshToServer(_ params : Dictionary<String, String>, completion: ((_ result:ServerResponse) -> Void)!){
        sendHTTPRequest(baseUrl+"/AccessControl/Access/Token", parameters: params as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    
    static func logoutFromServer(_ params : Dictionary<String, Dictionary<String, String>>,_ completion: ((_ result:ServerResponse) -> Void)!){
        
        sendHTTPRequest(baseUrl+"/AccessControl/Access/Logout", parameters: params as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    
    static func closeInstance(_ params : Dictionary<String, String>, completion: ((_ result:ServerResponse) -> Void)!){
        sendHTTPRequest(baseUrl+"/CheckList/Close", parameters: params as Dictionary<String, AnyObject>){ (result) -> Void in
            completion(result)
        }
    }
    
    static func sendClockRequest(_ params : Dictionary<String, AnyObject>, completion: ((_ result:ServerResponse) -> Void)!) {
        
        sendHTTPRequest(baseUrl+"/Server/Clock", parameters: params){ (result) -> Void in
            completion(result)
        }
    }
    
    
    static func getUrlEncodedParam(_ param: String) -> String? {
        let allowedCharacterSet = CharacterSet(charactersIn: "!*'();:@&=+$,/?%#[] ").inverted
        if let value = param.addingPercentEncoding(withAllowedCharacters: allowedCharacterSet) {
            print("enconded value is:", value)
            return value;
        }
        return nil
    }
    
    static func encodeParams(_ params: Dictionary<String, AnyObject>) -> Dictionary<String, AnyObject> {
        var dict : Dictionary<String, AnyObject> = Dictionary<String, AnyObject>()
        for (key, value) in params {
            print(key, ":", value.description!)
            if let param = value as? String {
                dict[key] = (getUrlEncodedParam(param) ?? param) as AnyObject
            } else if let param = value as? Dictionary<String, AnyObject> {
                dict[key] = encodeParams(param) as AnyObject
            }
        }
        return dict
    }
    
    static func sendHTTPRequest(_ urlPath: String,parameters : Dictionary<String, AnyObject>, completion: ((_ result:ServerResponse) -> Void)!){
        
        do {
            
            
            let request = NSMutableURLRequest(url: URL(string: urlPath)!)
            
            let session = URLSession.shared
            
            print("Parameters: \n", parameters.description)
            
            let params = encodeParams(parameters)
            
            request.httpMethod = "POST"
            
            request.timeoutInterval = 20
            
            
            
            if (!params.isEmpty)
                
            {
                
                
                if((params["Password"]) != nil) {
                    request.addValue("application/x-www-form-urlencoded", forHTTPHeaderField: "Content-Type")
                    
                    let bodyString = "grant_type=password" + "&companyId=" + (params["Company"] as! String) + "&username=" + (params["Code"] as! String) + "&password=" + (params["Password"] as! String)
                    
                    request.httpBody = bodyString.data(using: String.Encoding.utf8)
                    
                } else if((params["Refresh_Token"]) != nil) {
                    
                    request.addValue("application/x-www-form-urlencoded", forHTTPHeaderField: "Content-Type")
                    
                    let bodyString = "grant_type=refresh_token&refresh_token=" + (params["Refresh_Token"] as! String)
                    
                    request.httpBody = bodyString.data(using: String.Encoding.utf8)
                    
                } else {
                    request.addValue("application/json", forHTTPHeaderField: "Content-Type")
                    
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
                        
                        print((data != nil) ? String(decoding: data!, as: UTF8.self) : "")
                        
                        completion(serverResponse)
                    }
                }
            })
            
            task.resume()
        }
        catch{  print("error in sendHTTPRequest")  }
        
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
