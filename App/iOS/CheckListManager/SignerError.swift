//
//  SignerError.swift
//  Checklyst
//
//  Created by Matematici Developer on 09/06/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation

public enum SignerError: LocalizedError {
    case DuplicatedSigner(code: String), MainUser(code: String), InvalidSigner
    
    public var errorDescription: String? {
        switch self {
        case .DuplicatedSigner(let code):
            return "\(code) is already logged as signer."
        case .MainUser(let code):
            return "\(code) is logged as main user."
        case .InvalidSigner:
            return "Invalid user code."
        }
    }
}
