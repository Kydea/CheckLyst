//
//  SignersManager.swift
//  Checklyst
//
//  Created by Matematici Developer on 09/06/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation

class SignersManager {
    private static let signersIdentifier = "userSigners"
    
    private lazy var _signers: [Signer] = {
        return loadSigners()
    }()
    
    private static var instance: SignersManager?
    
    public static var shared: SignersManager {
        if instance == nil {
            instance = SignersManager()
        }
        return instance!
    }
    
    var signers: [Signer] {
        get {
            return _signers
        }
    }
    
    subscript(code: String) -> Signer? {
        return signers.first(where: { $0.code == code})
    }
    
    func addSigner(json:[String:Any]) {
        _signers.append(Signer(json: json))
        saveSigners()
    }
    
    func updateSigner(json:[String:Any]) {
        let code = json["code"] as! String
        guard let index = _signers.firstIndex(where: { $0.code == code}) else { return }
        _signers[index].accessToken = json["access_token"] as! String
        _signers[index].roles = json["roles"] as! String
        _signers[index].name = json["name"] as! String
        _signers[index].accessTokenExpiration = (Date() + (json["expires_in"] as! TimeInterval)).timeIntervalSince1970
    }
    
    func deleteSinger(_ signer: Signer) {
        guard let index = _signers.firstIndex(where: { $0.code == signer.code}) else { return }
        _signers.remove(at: index)
        saveSigners()
        
    }
    func clearSigners() {
        _signers = []
        UserDefaults.standard.set(nil, forKey: SignersManager.signersIdentifier)
    }
    
    private func saveSigners() {
        guard let data = try? JSONEncoder().encode(_signers) else {
            print("Error in saving signers")
            return
        }
        UserDefaults.standard.set(data, forKey: SignersManager.signersIdentifier)
        NotificationCenter.default.post(name: .SignersHaveBeenChanged, object: nil)
    }
    
    func checkSigner(code: String) throws {
        if code.isEmpty { throw SignerError.InvalidSigner }
        guard let index = _signers.firstIndex(where: { $0.code == code}) else {
            return
        }
        if code == UserDefaults.standard.string(forKey: "userCode") { throw SignerError.MainUser(code: code) }
        else { throw SignerError.DuplicatedSigner(code: code) }
    }
    
    private func loadSigners() -> [Signer] {
        guard let json = UserDefaults.standard.data(forKey: SignersManager.signersIdentifier),
              let signers = try? JSONDecoder().decode([Signer].self, from: json) else {
            return []
        }
        
        return signers
    }
}
